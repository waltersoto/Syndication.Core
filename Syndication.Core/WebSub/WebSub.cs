using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Syndication.Core.WebSub
{
    public class WebSubClient
    {
        private readonly HttpClient _httpClient;

        public WebSubClient(HttpClient? httpClient = null)
        {
            _httpClient = httpClient ?? new HttpClient();
        }

        public async Task<bool> SubscribeAsync(string hubUrl, string topicUrl, string callbackUrl, string? secret = null, int leaseSeconds = 86400, CancellationToken cancellationToken = default)
        {
            return await SendRequestAsync("subscribe", hubUrl, topicUrl, callbackUrl, secret, leaseSeconds, cancellationToken);
        }

        public async Task<bool> UnsubscribeAsync(string hubUrl, string topicUrl, string callbackUrl, string? secret = null, CancellationToken cancellationToken = default)
        {
            return await SendRequestAsync("unsubscribe", hubUrl, topicUrl, callbackUrl, secret, 0, cancellationToken);
        }

        private async Task<bool> SendRequestAsync(string mode, string hub, string topic, string callback, string? secret, int lease, CancellationToken cancellationToken)
        {
            var content = new FormUrlEncodedContent(new Dictionary<string, string?>
            {
                { "hub.mode", mode },
                { "hub.topic", topic },
                { "hub.callback", callback },
                { "hub.secret", secret }, // Optional but recommended
                { "hub.lease_seconds", lease.ToString() }
            });

            using (var response = await _httpClient.PostAsync(hub, content, cancellationToken))
            {
                return response.IsSuccessStatusCode;
            }
        }
    }

    public static class WebSubVerifier
    {
        public static bool VerifySignature(string text, string signatureHeader, string secret)
        {
            if (string.IsNullOrEmpty(signatureHeader) || string.IsNullOrEmpty(secret)) return false;

            // Header format: sha1=SIGNATURE
            var parts = signatureHeader.Split('=');
            if (parts.Length != 2) return false;

            var algo = parts[0];
            var hash = parts[1];

            byte[] computedHash;
            var bytes = Encoding.UTF8.GetBytes(text);
            var secretBytes = Encoding.UTF8.GetBytes(secret);

            if (algo.Equals("sha1", StringComparison.OrdinalIgnoreCase))
            {
                using (var hmac = new HMACSHA1(secretBytes))
                {
                    computedHash = hmac.ComputeHash(bytes);
                }
            }
            else if (algo.Equals("sha256", StringComparison.OrdinalIgnoreCase))
            {
                using (var hmac = new HMACSHA256(secretBytes))
                {
                    computedHash = hmac.ComputeHash(bytes);
                }
            }
            else
            {
                // Unsupported algorithm
                return false;
            }

            var computedHex = BitConverter.ToString(computedHash).Replace("-", "").ToLowerInvariant();
            return string.Equals(computedHex, hash, StringComparison.OrdinalIgnoreCase);
        }
    }
}
