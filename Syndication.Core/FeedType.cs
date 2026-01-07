namespace Syndication.Core
{
    /// <summary>
    /// Represents the type of syndication feed.
    /// </summary>
    public enum FeedType
    {
        /// <summary>
        /// Unknown or unsupported feed type.
        /// </summary>
        Unknown,

        /// <summary>
        /// RSS 2.0 or compatible 0.9x.
        /// </summary>
        Rss,

        /// <summary>
        /// Atom 1.0.
        /// </summary>
        Atom,

        /// <summary>
        /// JSON Feed.
        /// </summary>
        Json,

        /// <summary>
        /// ActivityPub (ActivityStreams 2.0).
        /// </summary>
        ActivityPub
    }
}
