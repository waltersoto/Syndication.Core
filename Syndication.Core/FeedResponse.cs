using System;

namespace Syndication.Core
{
    /// <summary>
    /// Represents the response from a feed request, including protocol metadata.
    /// </summary>
    public class FeedResponse
    {
        /// <summary>
        /// The parsed feed. Null if the status indicates not modified or failure.
        /// </summary>
        public Feed? Feed { get; set; }

        /// <summary>
        /// The HTTP status code.
        /// </summary>
        public int StatusCode { get; set; }

        /// <summary>
        /// The ETag value from the response header.
        /// </summary>
        public string? ETag { get; set; }

        /// <summary>
        /// The Last-Modified value from the response header.
        /// </summary>
        public DateTimeOffset? LastModified { get; set; }

        /// <summary>
        /// The content type of the response.
        /// </summary>
        public string? ContentType { get; set; }

        /// <summary>
        /// True if the request resulted in a 304 Not Modified.
        /// </summary>
        public bool NotModified => StatusCode == 304;
    }
}
