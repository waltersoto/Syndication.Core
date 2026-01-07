using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Syndication.Core
{
    /// <summary>
    /// Parser for RSS 1.0 (RDF).
    /// </summary>
    public class Rss10Parser : XmlFeedParser
    {
        private static readonly XNamespace RdfNs = "http://www.w3.org/1999/02/22-rdf-syntax-ns#";
        private static readonly XNamespace Rss10Ns = "http://purl.org/rss/1.0/";

        public override bool CanParse(string contentType, string contentSnippet)
        {
            if (!string.IsNullOrEmpty(contentSnippet) && contentSnippet.Contains("rdf:RDF") && contentSnippet.Contains("http://purl.org/rss/1.0/"))
            {
                return true;
            }
            return false;
        }

        protected override Task<Feed> ParseXDocumentAsync(XDocument document, CancellationToken cancellationToken)
        {
            if (document == null) throw new ArgumentNullException(nameof(document));

            var root = document.Root;
            if (root == null || !root.Name.LocalName.Equals("RDF", StringComparison.OrdinalIgnoreCase))
            {
                throw new ArgumentException("XML document is not an RSS 1.0 (RDF) feed.");
            }

            // Namespaces might vary in prefix, but URL is key.
            // Some might use default namespace.
            XNamespace rssNs = root.GetDefaultNamespace();
            if (rssNs == null || rssNs.NamespaceName == string.Empty) rssNs = Rss10Ns;

            var channel = root.Element(rssNs + "channel");
            if (channel == null) throw new ArgumentException("RSS 1.0 feed missing channel.");

            var feed = new Feed
            {
                Type = FeedType.Rss, // 1.0 is still RSS family
                Title = GetValue(channel, rssNs, "title"),
                Link = GetValue(channel, rssNs, "link"),
                Description = GetValue(channel, rssNs, "description"),
                ImageUrl = GetValue(channel.Element(rssNs + "image"), rssNs, "url")
            };

            // Items are children of rdf:RDF, generally siblings of channel.
            // Or they can be inside channel via <items><rdf:Seq>...
            // But we should look for <item> under root.
            foreach (var itemElem in root.Elements(rssNs + "item"))
            {
                var item = new FeedItem
                {
                    Title = GetValue(itemElem, rssNs, "title"),
                    Link = GetValue(itemElem, rssNs, "link"),
                    Description = GetValue(itemElem, rssNs, "description"),
                };
                
                // Dublin Core date often used
                var dcNs = XNamespace.Get("http://purl.org/dc/elements/1.1/");
                item.Published = ParseDate(GetValue(itemElem, dcNs, "date"));
                item.Author = GetValue(itemElem, dcNs, "creator");
                
                // content:encoded
                var contentNs = XNamespace.Get("http://purl.org/rss/1.0/modules/content/");
                item.Content = GetValue(itemElem, contentNs, "encoded");

                feed.Items.Add(item);
            }

            return Task.FromResult(feed);
        }

        private string? GetValue(XElement parent, XNamespace ns, string localName)
        {
            return parent?.Element(ns + localName)?.Value;
        }

        private DateTimeOffset? ParseDate(string? dateString)
        {
             if (string.IsNullOrWhiteSpace(dateString)) return null;
             if (DateTimeOffset.TryParse(dateString, out var date)) return date;
             return null;
        }
    }
}
