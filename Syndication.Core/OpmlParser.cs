using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace Syndication.Core
{
    public class OpmlParser
    {
        public async Task<OpmlDocument> ParseAsync(Stream stream, CancellationToken cancellationToken = default)
        {
            var settings = new XmlReaderSettings { Async = true, DtdProcessing = DtdProcessing.Ignore };
            using (var reader = XmlReader.Create(stream, settings))
            {
                var doc = await XDocument.LoadAsync(reader, LoadOptions.None, cancellationToken);
                var opml = doc.Root;
                if (opml == null || !opml.Name.LocalName.Equals("opml", StringComparison.OrdinalIgnoreCase))
                {
                    throw new ArgumentException("Not an OPML document.");
                }

                var head = opml.Element("head");
                var body = opml.Element("body");

                var document = new OpmlDocument
                {
                    Title = head?.Element("title")?.Value ?? string.Empty
                };

                if (body != null)
                {
                    document.Outlines = ParseOutlines(body.Elements("outline"));
                }
                
                return document;
            }
        }

        private List<OpmlOutline> ParseOutlines(IEnumerable<XElement> elements)
        {
            var list = new List<OpmlOutline>();
            foreach (var elem in elements)
            {
                var outline = new OpmlOutline
                {
                    Text = (string?)elem.Attribute("text") ?? (string?)elem.Attribute("title") ?? string.Empty,
                    Type = (string?)elem.Attribute("type") ?? string.Empty,
                    XmlUrl = (string?)elem.Attribute("xmlUrl") ?? string.Empty,
                    HtmlUrl = (string?)elem.Attribute("htmlUrl") ?? string.Empty
                };
                
                var children = elem.Elements("outline");
                if (children.Any())
                {
                     outline.Children = ParseOutlines(children);
                }
                
                list.Add(outline);
            }
            return list;
        }
    }
}
