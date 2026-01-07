using System;
using System.Collections.Generic;

namespace Syndication.Core
{
    /// <summary>
    /// Represents a syndication feed (RSS or Atom).
    /// </summary>
    public class Feed
    {
        /// <summary>
        /// Gets or sets the type of the feed.
        /// </summary>
        public FeedType Type { get; set; }

        /// <summary>
        /// Gets or sets the unique identifier for the feed (Atom ID, etc.).
        /// </summary>
        public string? Id { get; set; }

        /// <summary>
        /// Gets or sets the title of the feed.
        /// </summary>
        public string? Title { get; set; }

        /// <summary>
        /// Gets or sets the description or subtitle of the feed.
        /// </summary>
        public string? Description { get; set; }
        public string? Link { get; set; }
        /// <summary>
        /// Gets or sets the URL of the feed itself (link rel="self").
        /// </summary>
        public string? FeedLink { get; set; }

        /// <summary>
        /// Gets or sets the WebSub Hub URL (link rel="hub").
        /// </summary>
        public string? Hub { get; set; }
        public string? ImageUrl { get; set; }
        public string? Language { get; set; }
        public DateTimeOffset? LastUpdated { get; set; }
        public string? Copyright { get; set; }
        public string? Generator { get; set; }

        /// <summary>
        /// Gets or sets extracted items from the feed.
        /// </summary>
        public List<FeedItem> Items { get; set; } = new List<FeedItem>();

        /// <summary>
        /// Gets or sets extension data (e.g., custom namespaces, JSON fields).
        /// </summary>
        public Dictionary<string, object> Extensions { get; set; } = new Dictionary<string, object>();
    }
}
