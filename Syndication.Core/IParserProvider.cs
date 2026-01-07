using System.Collections.Generic;

namespace Syndication.Core
{
    /// <summary>
    /// Registry for feed parsers.
    /// </summary>
    public interface IParserProvider
    {
        /// <summary>
        /// Registers a new parser.
        /// </summary>
        /// <param name="parser">The parser to register.</param>
        void RegisterParser(IFeedParser parser);

        /// <summary>
        /// Gets the best matching parser for the content.
        /// </summary>
        /// <param name="contentType">The MIME type.</param>
        /// <param name="contentSnippet">The beginning of the content.</param>
        /// <returns>A matching parser or null.</returns>
        IFeedParser? GetParser(string contentType, string contentSnippet);
        
        /// <summary>
        /// Gets all registered parsers.
        /// </summary>
        IEnumerable<IFeedParser> GetParsers();
    }
}
