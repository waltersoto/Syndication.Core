using System;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Syndication.Core
{
    /// <summary>
    /// The main entry point for reading syndication feeds.
    /// </summary>
    public class FeedReader
    {
        private readonly HttpClient _httpClient;
        private readonly IParserProvider _parserProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="FeedReader"/> class.
        /// </summary>
        /// <param name="httpClient">Optional HttpClient.</param>
        /// <param name="parserProvider">Optional ParserProvider. If null, a default one with RssParser and AtomParser is used.</param>
        public FeedReader(HttpClient? httpClient = null, IParserProvider? parserProvider = null)
        {
            if (httpClient == null)
            {
                var handler = new HttpClientHandler
                {
                    AutomaticDecompression = System.Net.DecompressionMethods.All
                };
                _httpClient = new HttpClient(handler);
            }
            else
            {
                _httpClient = httpClient;
            }
            
            if (parserProvider == null)
            {
                var provider = new ParserProvider();
                provider.RegisterParser(new RssParser());
                provider.RegisterParser(new AtomParser());
                provider.RegisterParser(new JsonFeedParser());
                provider.RegisterParser(new ActivityPubParser());
                provider.RegisterParser(new Rss10Parser());
                provider.RegisterParser(new HtmlSemanticParser());
                _parserProvider = provider;
            }
            else
            {
                _parserProvider = parserProvider;
            }
        }

        /// <summary>
        /// Reads a feed from the specified URL asynchronously.
        /// </summary>
        /// <param name="url">The URL of the feed.</param>
        /// <param name="etag">Optional ETag from previous request for Conditional GET.</param>
        /// <param name="lastModified">Optional Last-Modified from previous request for Conditional GET.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A FeedResponse containing the feed and metadata.</returns>
        public async Task<FeedResponse> ReadUrlAsync(string url, string? etag = null, DateTimeOffset? lastModified = null, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(url)) throw new ArgumentException("Url cannot be null or empty.", nameof(url));

            var request = new HttpRequestMessage(HttpMethod.Get, url);
            
            // Compression is usually handled by HttpClientHandler (AutomaticDecompression), 
            // but we can ensure headers are set if the user didn't configure a handler.
            // However, we can't easily modify the inner handler of an existing HttpClient. 
            // We assume the user or the default constructor set it up correctly. 
            // Ideally, we'd add "Accept-Encoding: gzip, br" but HttpClient does this automatically if Decompression is enabled.

            if (!string.IsNullOrEmpty(etag))
            {
                request.Headers.IfNoneMatch.TryParseAdd(etag);
            }

            if (lastModified.HasValue)
            {
                request.Headers.IfModifiedSince = lastModified.Value;
            }

            using (var response = await _httpClient.SendAsync(request, cancellationToken))
            {
                var result = new FeedResponse
                {
                    StatusCode = (int)response.StatusCode,
                    ContentType = response.Content.Headers.ContentType?.MediaType,
                    ETag = response.Headers.ETag?.Tag,
                    LastModified = response.Content.Headers.LastModified
                };

                if (response.StatusCode == System.Net.HttpStatusCode.NotModified)
                {
                    return result;
                }

                response.EnsureSuccessStatusCode();
                
                // Discovery Logic
                // If content type is HTML, or if parsing fails, try to discover feeds.
                bool isHtml = result.ContentType != null && (result.ContentType.Contains("text/html") || result.ContentType.Contains("application/xhtml+xml"));

                // We need to read the stream.
                // If it's HTML, we read as string for discovery.
                // If it's potentially a feed, we read as stream for parsing.
                // To support both (try parse, then fallback), we copy to memory.
                
                var memoryStream = new MemoryStream();
                using (var stream = await response.Content.ReadAsStreamAsync())
                {
                    await stream.CopyToAsync(memoryStream, cancellationToken);
                }
                memoryStream.Position = 0;

                try
                {
                    // If strictly HTML, maybe skip direct parse attempt? 
                    // But some feeds serve as text/html erroneously. 
                    // Let's try parsing first if it looks slightly like XML/JSON, or if we want to be aggressive.
                    // But if it IS a valid HTML page, parsing as XML Feed might fail or produce garbage.
                    // Check sniffing.
                    
                    if (!isHtml)
                    {
                         result.Feed = await ReadStreamAsync(memoryStream, cancellationToken, result.ContentType);
                         return result;
                    }
                }
                catch (Exception)
                {
                    // Parsing failed, proceed to discovery if HTML
                    if (!isHtml) throw; 
                }

                // If we are here, it's either HTML or parsing passed (but logic above returns).
                // Actually, if parsing failed and it wasn't HTML, we rethrow.
                // So here we only care if it IS HTML (or we want to fallback).
                
                if (isHtml)
                {
                    memoryStream.Position = 0;
                    using (var reader = new StreamReader(memoryStream))
                    {
                        var html = await reader.ReadToEndAsync();
                        var discoverer = new HtmlFeedDiscoverer();
                        var feeds = discoverer.DiscoverFeedUrls(html, url);
                        
                        var discoveredUrl = feeds.FirstOrDefault();
                        if (discoveredUrl != null)
                        {
                            // Follow the discovered URL
                            return await ReadUrlAsync(discoveredUrl, etag, lastModified, cancellationToken);
                        }
                    }
                    
                    // No feed found in HTML via Discovery.
                    // Fallback: Try to parse the HTML string itself using a registered parser (e.g. HtmlSemanticParser)
                    try
                    {
                        memoryStream.Position = 0;
                        result.Feed = await ReadStreamAsync(memoryStream, cancellationToken, result.ContentType);
                        return result;
                    }
                    catch (Exception)
                    {
                        // Fallback failed too. Result.Feed remains null.
                    }
                }
                
                return result;
            }
        }

        /// <summary>
        /// Reads a feed from a stream asynchronously.
        /// </summary>
        public async Task<Feed> ReadStreamAsync(Stream stream, CancellationToken cancellationToken = default, string? contentType = null)
        {
            if (stream == null) throw new ArgumentNullException(nameof(stream));

            // We need to peek at the stream for sniffing.
            // Copy to MemoryStream to ensure seekability and safe peeking.
            var memoryStream = new MemoryStream();
            await stream.CopyToAsync(memoryStream, cancellationToken);
            memoryStream.Position = 0;

            // Sniff buffer
            var buffer = new byte[512];
            var bytesRead = await memoryStream.ReadAsync(buffer, 0, buffer.Length, cancellationToken);
            var header = Encoding.UTF8.GetString(buffer, 0, bytesRead); // Assuming UTF8 for sniffing is mostly safe for XML tags
            
            memoryStream.Position = 0;

            var parser = _parserProvider.GetParser(contentType ?? string.Empty, header);
            if (parser == null)
            {
                 throw new NotSupportedException($"Unknown feed format. ContentType: {contentType}, Header snippet: {header.Substring(0, Math.Min(50, header.Length))}...");
            }

            return await parser.ParseAsync(memoryStream, cancellationToken);
        }

        /// <summary>
        /// Reads a feed from a string asynchronously.
        /// </summary>
        public async Task<Feed> ReadStringAsync(string xml, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(xml)) throw new ArgumentException("Xml cannot be null or empty.", nameof(xml));

            using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(xml)))
            {
                return await ReadStreamAsync(stream, cancellationToken);
            }
        }
        
        /// <summary>
        /// Registers a new parser type dynamically.
        /// </summary>
        public void RegisterParser(IFeedParser parser)
        {
            _parserProvider.RegisterParser(parser);
        }
    }
}
