using System;

namespace Syndication.Core
{
    /// <summary>
    /// Represents a single item in a syndication feed.
    /// </summary>
    public class FeedItem
    {
        /// <summary>
        /// Gets or sets the unique identifier for the item (GUID, URI, etc.).
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets the title of the item.
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Gets or sets the description or summary of the item.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the content of the item (often HTML).
        /// </summary>
        public string Content { get; set; }

        /// <summary>
        /// Gets or sets the link to the item.
        /// </summary>
        public string Link { get; set; }

        /// <summary>
        /// Gets or sets the author of the item.
        /// </summary>
        public string Author { get; set; }

        /// <summary>
        /// Gets or sets the categories or tags associated with the item.
        /// </summary>
        public string[] Categories { get; set; }

        /// <summary>
        /// Gets or sets the publication date of the item.
        /// </summary>
        public DateTimeOffset? Published { get; set; }

        /// <summary>
        /// Gets or sets the last updated date of the item.
        /// </summary>
        public DateTimeOffset? Updated { get; set; }

        /// <summary>
        /// Gets or sets extension data.
        /// </summary>
        public Dictionary<string, object> Extensions { get; set; } = new Dictionary<string, object>();
    }
}
