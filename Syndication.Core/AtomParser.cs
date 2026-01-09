using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Syndication.Core
{
    /// <summary>
    /// Parser for Atom 1.0 feeds.
    /// </summary>
    public class AtomParser : XmlFeedParser
    {
        private static readonly XNamespace AtomNs = "http://www.w3.org/2005/Atom";

        public override bool CanParse(string contentType, string contentSnippet)
        {
            if (!string.IsNullOrEmpty(contentType) && contentType.Contains("atom")) return true;
            if (!string.IsNullOrEmpty(contentSnippet) && contentSnippet.Contains("<feed") && contentSnippet.Contains("http://www.w3.org/2005/Atom")) return true;
            return false;
        }

        protected override Task<Feed> ParseXDocumentAsync(XDocument document, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(document);

            var root = document.Root;
            if (root == null || !root.Name.LocalName.Equals("feed", StringComparison.OrdinalIgnoreCase))
            {
                // Fallback check?
                throw new ArgumentException("XML document is not an Atom feed.");
            }

            var ns = root.GetDefaultNamespace();

            var feed = new Feed
            {
                Type = FeedType.Atom,
                Title = GetValue(root, ns, "title"),
                Description = GetValue(root, ns, "subtitle"),
                Id = GetValue(root, ns, "id"),
                LastUpdated = ParseDate(GetValue(root, ns, "updated")),
                Copyright = GetValue(root, ns, "rights"),
                Generator = GetValue(root, ns, "generator"),
                ImageUrl = GetValue(root, ns, "logo") ?? GetValue(root, ns, "icon"),
                FeedLink = GetLink(root, ns, "self"),
                Hub = GetLink(root, ns, "hub"),
                Link = GetLink(root, ns, "alternate") ?? GetLink(root, ns, null) // Default link
            };

            foreach (var entry in root.Elements(ns + "entry"))
            {
                var item = new FeedItem
                {
                    Title = GetValue(entry, ns, "title"),
                    Id = GetValue(entry, ns, "id"),
                    Published = ParseDate(GetValue(entry, ns, "published")),
                    Updated = ParseDate(GetValue(entry, ns, "updated")),
                    Author = GetAuthor(entry, ns),
                    Link = GetLink(entry, ns, "alternate") ?? GetLink(entry, ns, null),
                    Categories = entry.Elements(ns + "category").Select(x => (string?)x.Attribute("term")).Where(x => x != null).ToArray()!
                };

                var content = GetValue(entry, ns, "content");
                var summary = GetValue(entry, ns, "summary");

                item.Content = content ?? summary;
                item.Description = summary ?? content;

                feed.Items.Add(item);
            }

            return Task.FromResult(feed);
        }

        private string? GetValue(XElement parent, XNamespace ns, string localName)
        {
            return parent.Element(ns + localName)?.Value;
        }

        private string? GetLink(XElement parent, XNamespace ns, string? rel)
        {
            var links = parent.Elements(ns + "link");
            if (rel == null)
            {
                // Return first link without rel or rel="alternate"
                return (string?)links.FirstOrDefault(l => l.Attribute("rel") == null || (string?)l.Attribute("rel") == "alternate")?.Attribute("href");
            }
            return (string?)links.FirstOrDefault(l => (string?)l.Attribute("rel") == rel)?.Attribute("href");
        }

        private string? GetAuthor(XElement parent, XNamespace ns)
        {
            var author = parent.Element(ns + "author");
            return author?.Element(ns + "name")?.Value;
        }

        private DateTimeOffset? ParseDate(string? dateString)
        {
            if (string.IsNullOrWhiteSpace(dateString)) return null;
            if (DateTimeOffset.TryParse(dateString, out var date)) return date;
            return null;
        }
    }
}
