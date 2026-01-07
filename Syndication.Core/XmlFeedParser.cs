using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace Syndication.Core
{
    /// <summary>
    /// Base class for XML-based feed parsers (RSS, Atom).
    /// </summary>
    public abstract class XmlFeedParser : IFeedParser
    {
        public abstract bool CanParse(string contentType, string contentSnippet);

        public async Task<Feed> ParseAsync(Stream stream, CancellationToken cancellationToken = default)
        {
             // Use XmlReader for safety and XDocument for ease
            var settings = new XmlReaderSettings
            {
                Async = true,
                DtdProcessing = DtdProcessing.Ignore // Security: prevent XXE
            };

            using (var reader = XmlReader.Create(stream, settings))
            {
                var doc = await XDocument.LoadAsync(reader, LoadOptions.None, cancellationToken);
                return await ParseXDocumentAsync(doc, cancellationToken);
            }
        }

        protected abstract Task<Feed> ParseXDocumentAsync(XDocument document, CancellationToken cancellationToken);
    }
}
