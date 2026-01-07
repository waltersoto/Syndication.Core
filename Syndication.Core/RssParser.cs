using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Syndication.Core
{
    /// <summary>
    /// Parser for RSS 2.0 feeds.
    /// </summary>
    public class RssParser : XmlFeedParser
    {
        public override bool CanParse(string contentType, string contentSnippet)
        {
            // Simple sniffing
            if (!string.IsNullOrEmpty(contentType) && contentType.Contains("rss")) return true;
            if (!string.IsNullOrEmpty(contentSnippet) && contentSnippet.Contains("<rss")) return true;
            return false;
        }

        protected override Task<Feed> ParseXDocumentAsync(XDocument document, CancellationToken cancellationToken)
        {
            if (document == null) throw new ArgumentNullException(nameof(document));

            var rss = document.Root;
            if (rss == null || !rss.Name.LocalName.Equals("rss", StringComparison.OrdinalIgnoreCase))
            {
                throw new ArgumentException("XML document is not an RSS feed.");
            }

            var channel = rss.Element("channel");
            if (channel == null)
            {
                throw new ArgumentException("RSS feed is missing the 'channel' element.");
            }

            var feed = new Feed
            {
                Type = FeedType.Rss,
                Title = GetElementValue(channel, "title"),
                Link = GetElementValue(channel, "link"),
                Description = GetElementValue(channel, "description"),
                Language = GetElementValue(channel, "language"),
                Copyright = GetElementValue(channel, "copyright"),
                Generator = GetElementValue(channel, "generator"),
                ImageUrl = GetElementValue(channel.Element("image"), "url"),
                LastUpdated = ParseDate(GetElementValue(channel, "lastBuildDate") ?? GetElementValue(channel, "pubDate"))
            };

            // Atom link in RSS (common extension)
            var atomNs = XNamespace.Get("http://www.w3.org/2005/Atom");
            var atomLink = channel.Elements(atomNs + "link")
                .FirstOrDefault(x => (string)x.Attribute("rel") == "self");
            if (atomLink != null)
            {
                feed.FeedLink = (string)atomLink.Attribute("href");
            }

            var hubLink = channel.Elements(atomNs + "link")
                .FirstOrDefault(x => (string)x.Attribute("rel") == "hub");
            if (hubLink != null)
            {
                feed.Hub = (string)hubLink.Attribute("href");
            }

            foreach (var itemElement in channel.Elements("item"))
            {
                var item = new FeedItem
                {
                    Title = GetElementValue(itemElement, "title"),
                    Link = GetElementValue(itemElement, "link"),
                    Description = GetElementValue(itemElement, "description"),
                    Author = GetElementValue(itemElement, "author") ?? GetElementValue(itemElement, "creator", XNamespace.Get("http://purl.org/dc/elements/1.1/")), // Dublin Core
                    Id = GetElementValue(itemElement, "guid"),
                    Published = ParseDate(GetElementValue(itemElement, "pubDate")),
                    Categories = itemElement.Elements("category").Select(x => x.Value).ToArray()
                };

                // content:encoded
                var contentNs = XNamespace.Get("http://purl.org/rss/1.0/modules/content/");
                item.Content = GetElementValue(itemElement, "encoded", contentNs) ?? item.Description;

                // EXTENSIONS
                
                // iTunes
                ParsePodcastItem(item, itemElement);
                
                // Media RSS
                ParseMediaItem(item, itemElement);

                feed.Items.Add(item);
            }

            // Feed Extensions
            ParsePodcastFeed(feed, channel);

            return Task.FromResult(feed);
        }

        private void ParsePodcastFeed(Feed feed, XElement channel)
        {
            var ns = XNamespace.Get(Extensions.PodcastKeys.Namespace);
            var data = new Extensions.PodcastFeedData
            {
                Author = GetElementValue(channel, "author", ns),
                Subtitle = GetElementValue(channel, "subtitle", ns),
                Summary = GetElementValue(channel, "summary", ns),
                ImageUrl = channel.Element(ns + "image")?.Attribute("href")?.Value,
                Explicit = GetElementValue(channel, "explicit", ns) == "yes"
            };

            var owner = channel.Element(ns + "owner");
            if (owner != null)
            {
                data.OwnerName = GetElementValue(owner, "name", ns);
                data.OwnerEmail = GetElementValue(owner, "email", ns);
            }

            // Simple category support (top level)
            var cat = channel.Element(ns + "category");
            if (cat != null)
            {
                data.Category = (string?)cat.Attribute("text");
            }
            
            // Only add if we found something relevant
            if (!string.IsNullOrEmpty(data.Author) || !string.IsNullOrEmpty(data.Summary))
            {
                feed.Extensions[Extensions.PodcastKeys.ExtensionKey] = data;
            }
        }

        private void ParsePodcastItem(FeedItem item, XElement element)
        {
            var ns = XNamespace.Get(Extensions.PodcastKeys.Namespace);
            var data = new Extensions.PodcastItemData
            {
                Subtitle = GetElementValue(element, "subtitle", ns),
                Summary = GetElementValue(element, "summary", ns),
                ImageUrl = element.Element(ns + "image")?.Attribute("href")?.Value,
                EpisodeType = GetElementValue(element, "episodeType", ns),
                Explicit = GetElementValue(element, "explicit", ns) == "yes"
            };

            var dur = GetElementValue(element, "duration", ns);
            if (!string.IsNullOrEmpty(dur))
            {
                data.Initial_Duration = dur;
                // Try parse timespan (HH:MM:SS or integer seconds)
                if (int.TryParse(dur, out var seconds))
                {
                    data.Duration = TimeSpan.FromSeconds(seconds);
                }
                else if (TimeSpan.TryParse(dur, out var ts))
                {
                    data.Duration = ts;
                }
            }
            
            if (int.TryParse(GetElementValue(element, "episode", ns), out var ep)) data.Episode = ep;
            if (int.TryParse(GetElementValue(element, "season", ns), out var s)) data.Season = s;

            if (!string.IsNullOrEmpty(data.Summary) || data.Duration.HasValue)
            {
                item.Extensions[Extensions.PodcastKeys.ExtensionKey] = data;
            }
        }

        private void ParseMediaItem(FeedItem item, XElement element)
        {
            var ns = XNamespace.Get(Extensions.MediaKeys.Namespace);
            var data = new Extensions.MediaItemData
            {
                Title = GetElementValue(element, "title", ns),
                Description = GetElementValue(element, "description", ns),
            };

            var thumb = element.Elements(ns + "thumbnail").FirstOrDefault();
            if (thumb != null)
            {
                data.ThumbnailUrl = (string?)thumb.Attribute("url");
            }

            foreach (var content in element.Elements(ns + "content"))
            {
                var mc = new Extensions.MediaContent
                {
                    Url = (string?)content.Attribute("url"),
                    Type = (string?)content.Attribute("type"),
                    Medium = (string?)content.Attribute("medium"),
                };
                
                if (long.TryParse((string?)content.Attribute("fileSize"), out var sz)) mc.FileSize = sz;
                if (int.TryParse((string?)content.Attribute("width"), out var w)) mc.Width = w;
                if (int.TryParse((string?)content.Attribute("height"), out var h)) mc.Height = h;

                data.Contents.Add(mc);
            }

            if (data.Contents.Any() || !string.IsNullOrEmpty(data.ThumbnailUrl))
            {
                item.Extensions[Extensions.MediaKeys.ExtensionKey] = data;
            }
        }

        private string? GetElementValue(XElement? parent, string localName, XNamespace? ns = null)
        {
            if (parent == null) return null;
            XElement? element;
            if (ns == null)
            {
                element = parent.Element(localName);
            }
            else
            {
                element = parent.Element(ns + localName);
            }
            return element?.Value;
        }

        private DateTimeOffset? ParseDate(string? dateString)
        {
            if (string.IsNullOrWhiteSpace(dateString)) return null;

            if (DateTimeOffset.TryParse(dateString, out var date))
            {
                return date;
            }
            
            return null;
        }
    }
}
