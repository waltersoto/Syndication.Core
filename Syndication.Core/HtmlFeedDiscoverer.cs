using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Syndication.Core
{
    public class HtmlFeedDiscoverer
    {
        // Simple regex to find link tags. 
        // Note: Regex parsing HTML is fragile, but standard for lightweight discovery.
        private static readonly Regex LinkTagRegex = new Regex(@"<link[^>]+>", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        private static readonly Regex RelRegex = new Regex(@"rel=[""']\s*alternate\s*[""']", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        private static readonly Regex HrefRegex = new Regex(@"href=[""']([^""']+)[""']", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        private static readonly Regex TypeRegex = new Regex(@"type=[""']([^""']+)[""']", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        public IEnumerable<string> DiscoverFeedUrls(string html, string baseUrl)
        {
            var matches = LinkTagRegex.Matches(html);
            foreach (Match match in matches)
            {
                var tag = match.Value;
                
                // Must have rel="alternate"
                if (!RelRegex.IsMatch(tag)) continue;

                // Check type
                var typeMatch = TypeRegex.Match(tag);
                if (!typeMatch.Success) continue;
                
                var type = typeMatch.Groups[1].Value.ToLowerInvariant();
                if (IsFeedType(type))
                {
                    var hrefMatch = HrefRegex.Match(tag);
                    if (hrefMatch.Success)
                    {
                        var href = hrefMatch.Groups[1].Value;
                        // Resolve relative URL
                        if (Uri.TryCreate(new Uri(baseUrl), href, out var result))
                        {
                            yield return result.ToString();
                        }
                    }
                }
            }
        }

        private bool IsFeedType(string type)
        {
            return type.Contains("application/rss+xml") ||
                   type.Contains("application/atom+xml") ||
                   type.Contains("application/feed+json") ||
                   type.Contains("application/json") || 
                   type.Contains("text/xml"); // Some old RSS feeds
        }
    }
}
