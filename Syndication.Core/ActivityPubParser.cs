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
    /// Parser for ActivityPub / ActivityStreams 2.0 feeds (e.g., Mastodon outboxes).
    /// </summary>
    public class ActivityPubParser : IFeedParser
    {
        public bool CanParse(string contentType, string contentSnippet)
        {
            if (!string.IsNullOrEmpty(contentType) && contentType.Contains("application/activity+json"))
            {
                return true;
            }
            // Snippet check: Look for context
            if (!string.IsNullOrEmpty(contentSnippet) && contentSnippet.Contains("https://www.w3.org/ns/activitystreams"))
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
                throw new ArgumentException("Invalid JSON content for ActivityPub.", ex);
            }

            var root = doc.RootElement;
            if (root.ValueKind != JsonValueKind.Object)
            {
                 throw new ArgumentException("ActivityPub root must be an object.");
            }

            // Must have context or type
            // Usually type: OrderedCollection or OrderedCollectionPage for feeds
            
            var feed = new Feed
            {
                Type = FeedType.ActivityPub, 
                // ActivityPub doesn't always have a title for the feed itself in the Outbox JSON, 
                // often it's just a collection. We might treat it as "ActivityPub Feed".
                Title = "ActivityPub Feed", 
                Description = GetString(root, "summary")
            };
            
            feed.Extensions["ActivityPub"] = true;

            JsonElement itemsArray = default;
            
            // Check for 'orderedItems' or 'items'
            if (root.TryGetProperty("orderedItems", out var ordered)) itemsArray = ordered;
            else if (root.TryGetProperty("items", out var items)) itemsArray = items;
            
            // Should also handle 'first' paging to get actual items if this is just the collection root?
            // For simplicity, we assume we got a page or a collection with items inlined.
            
            if (itemsArray.ValueKind == JsonValueKind.Array)
            {
                foreach (var itemElem in itemsArray.EnumerateArray())
                {
                    // Items can be Create activities or the Objects themselves.
                    // If type is "Create", look at "object".
                    
                    JsonElement objectElem = itemElem;
                    var type = GetString(itemElem, "type");
                    
                    if (type == "Create" || type == "Announce") // Announce is a Boost
                    {
                        if (itemElem.TryGetProperty("object", out var obj))
                        {
                            // If object is just a string URL, we can't really parse it without fetching.
                            // We skip those for now.
                            if (obj.ValueKind == JsonValueKind.Object)
                            {
                                objectElem = obj;
                            }
                        }
                    }

                    var item = new FeedItem
                    {
                        Id = GetString(objectElem, "id"),
                        Title = GetString(objectElem, "name") ?? GetString(objectElem, "summary") ?? "Untitled Note",
                        Content = GetString(objectElem, "content"),
                        Link = GetString(objectElem, "url"),
                        Published = ParseDate(GetString(objectElem, "published")),
                        Updated = ParseDate(GetString(objectElem, "updated")),
                        Author = GetString(objectElem, "attributedTo") // This is usually a URL in AP
                    };
                    
                    // Attachments (Images)
                    if (objectElem.TryGetProperty("attachment", out var attArray) && attArray.ValueKind == JsonValueKind.Array)
                    {
                        // Map first image to Media extension if needed?
                        // Or just store in extensions.
                    }

                    feed.Items.Add(item);
                }
            }

            return feed;
        }

        private string? GetString(JsonElement element, string propName)
        {
            if (element.ValueKind == JsonValueKind.Object && element.TryGetProperty(propName, out var prop) && prop.ValueKind == JsonValueKind.String)
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
