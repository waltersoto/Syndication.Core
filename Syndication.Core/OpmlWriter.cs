using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;

namespace Syndication.Core
{
    public class OpmlWriter
    {
        public async Task WriteAsync(OpmlDocument document, Stream stream, CancellationToken cancellationToken = default)
        {
            if (document == null) throw new ArgumentNullException(nameof(document));
            if (stream == null) throw new ArgumentNullException(nameof(stream));

            var settings = new XmlWriterSettings
            {
                Async = true,
                Indent = true,
                Encoding = System.Text.Encoding.UTF8
            };

            using (var writer = XmlWriter.Create(stream, settings))
            {
                await writer.WriteStartDocumentAsync();
                await writer.WriteStartElementAsync(null, "opml", null);
                await writer.WriteAttributeStringAsync(null, "version", null, "2.0");

                // Head
                await writer.WriteStartElementAsync(null, "head", null);
                await writer.WriteElementStringAsync(null, "title", null, document.Title);
                await writer.WriteElementStringAsync(null, "dateCreated", null, DateTime.UtcNow.ToString("R"));
                await writer.WriteEndElementAsync(); // head

                // Body
                await writer.WriteStartElementAsync(null, "body", null);
                if (document.Outlines != null)
                {
                    foreach (var outline in document.Outlines)
                    {
                        await WriteOutlineAsync(writer, outline);
                    }
                }
                await writer.WriteEndElementAsync(); // body

                await writer.WriteEndElementAsync(); // opml
                await writer.WriteEndDocumentAsync();
                await writer.FlushAsync();
            }
        }

        private async Task WriteOutlineAsync(XmlWriter writer, OpmlOutline outline)
        {
            await writer.WriteStartElementAsync(null, "outline", null);
            await writer.WriteAttributeStringAsync(null, "text", null, outline.Text);
            
            if (!string.IsNullOrEmpty(outline.Type))
                await writer.WriteAttributeStringAsync(null, "type", null, outline.Type);
            
            if (!string.IsNullOrEmpty(outline.XmlUrl))
                await writer.WriteAttributeStringAsync(null, "xmlUrl", null, outline.XmlUrl);
                
            if (!string.IsNullOrEmpty(outline.HtmlUrl))
                await writer.WriteAttributeStringAsync(null, "htmlUrl", null, outline.HtmlUrl);

            if (outline.Children != null && outline.Children.Count > 0)
            {
                foreach (var child in outline.Children)
                {
                    await WriteOutlineAsync(writer, child);
                }
            }
            
            await writer.WriteEndElementAsync(); // outline
        }
    }
}
