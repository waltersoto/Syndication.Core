using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Syndication.Core
{
    /// <summary>
    /// Interface for feed parsers.
    /// </summary>
    public interface IFeedParser
    {
        /// <summary>
        /// Determines whether this parser can parse the given content.
        /// </summary>
        /// <param name="contentType">The MIME type of the content (if available).</param>
        /// <param name="contentSnippet">The first few characters/bytes of the content for sniffing.</param>
        /// <returns>True if the parser recognizes the content; otherwise, false.</returns>
        bool CanParse(string contentType, string contentSnippet);

        /// <summary>
        /// Parses the feed from the provided stream.
        /// </summary>
        /// <param name="stream">The stream containing the feed.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The parsed Feed.</returns>
        Task<Feed> ParseAsync(Stream stream, CancellationToken cancellationToken = default);
    }
}
