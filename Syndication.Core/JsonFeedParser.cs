using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Syndication.Core
{
    /// <summary>
    /// Parser for JSON Feed (v1, v1.1).
    /// </summary>
    public class JsonFeedParser : IFeedParser
    {
        public bool CanParse(string contentType, string contentSnippet)
        {
            if (!string.IsNullOrEmpty(contentType) && (contentType.Contains("application/feed+json") || contentType.Contains("application/json")))
            {
                return true;
            }
            if (!string.IsNullOrEmpty(contentSnippet) && contentSnippet.TrimStart().StartsWith("{") && contentSnippet.Contains("https://jsonfeed.org/version/"))
            {
                return true;
            }
            return false;
        }

        public async Task<Feed> ParseAsync(Stream stream, CancellationToken cancellationToken = default)
        {
            JsonDocument doc;
            try
            {
                doc = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken);
            }
            catch (JsonException ex)
            {
                throw new ArgumentException("Invalid JSON content.", ex);
            }

            var root = doc.RootElement;
            if (root.ValueKind != JsonValueKind.Object)
            {
                 throw new ArgumentException("JSON Feed root must be an object.");
            }

            if (!root.TryGetProperty("version", out var versionProp) || !versionProp.GetString()!.Contains("jsonfeed.org"))
            {
                // Strict check? Or lenient?
                // Depending on detection logic, we might already be confident.
            }

            var feed = new Feed
            {
                Type = FeedType.Json,
                Title = GetString(root, "title"),
                Description = GetString(root, "description"),
                Link = GetString(root, "home_page_url"),
                FeedLink = GetString(root, "feed_url"),
                ImageUrl = GetString(root, "icon") ?? GetString(root, "favicon"),
                // JSON Feed doesn't strongly spec language/copyright/generator in top level standard fields usually, but sometimes extensions.
                // We'll map what fits.
            };
            
            // Allow extension data
            // feed.Extensions = ... (could iterate all props and put unknown ones in extensions)

            if (root.TryGetProperty("items", out var itemsProp) && itemsProp.ValueKind == JsonValueKind.Array)
            {
                foreach (var itemElem in itemsProp.EnumerateArray())
                {
                    var item = new FeedItem
                    {
                        Id = GetString(itemElem, "id"),
                        Title = GetString(itemElem, "title"),
                        Link = GetString(itemElem, "url"), // JSON Feed uses 'url' for the permalink
                        Content = GetString(itemElem, "content_html") ?? GetString(itemElem, "content_text"),
                        Description = GetString(itemElem, "summary"),
                        Published = ParseDate(GetString(itemElem, "date_published")),
                        Updated = ParseDate(GetString(itemElem, "date_modified")),
                        // Author in JSON feed is object or list.
                    };
                    
                    if (itemElem.TryGetProperty("author", out var authorProp))
                    {
                         if(authorProp.ValueKind == JsonValueKind.Object)
                         {
                             item.Author = GetString(authorProp, "name");
                         }
                    }
                    else if (itemElem.TryGetProperty("authors", out var authorsProp) && authorsProp.ValueKind == JsonValueKind.Array)
                    {
                        var authorNames = new List<string>();
                        foreach(var auth in authorsProp.EnumerateArray())
                        {
                            var name = GetString(auth, "name");
                            if(!string.IsNullOrEmpty(name)) authorNames.Add(name);
                        }
                        if (authorNames.Any()) item.Author = string.Join(", ", authorNames);
                    }

                    if (itemElem.TryGetProperty("tags", out var tagsProp) && tagsProp.ValueKind == JsonValueKind.Array)
                    {
                        item.Categories = tagsProp.EnumerateArray().Select(x => x.GetString()).Where(x => x != null).ToArray()!;
                    }

                    feed.Items.Add(item);
                }
            }

            return feed;
        }

        private string? GetString(JsonElement element, string propName)
        {
            if (element.TryGetProperty(propName, out var prop) && prop.ValueKind == JsonValueKind.String)
            {
                return prop.GetString();
            }
            return null;
        }

        private DateTimeOffset? ParseDate(string? dateString)
        {
             if (string.IsNullOrWhiteSpace(dateString)) return null;
             if (DateTimeOffset.TryParse(dateString, out var date)) return date;
             return null;
        }
    }
}
