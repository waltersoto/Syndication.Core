using System.Collections.Generic;

namespace Syndication.Core.Extensions
{
    public static class MediaKeys
    {
        public const string Namespace = "http://search.yahoo.com/mrss/";
        public const string ExtensionKey = "media";
    }

    public class MediaItemData
    {
        public List<MediaContent> Contents { get; set; } = new List<MediaContent>();
        public string? Title { get; set; }
        public string? Description { get; set; }
        public string? ThumbnailUrl { get; set; }
    }

    public class MediaContent
    {
        public string? Url { get; set; }
        public string? Type { get; set; }
        public long? FileSize { get; set; }
        public string? Medium { get; set; } // image, audio, video
        public int? Width { get; set; }
        public int? Height { get; set; }
    }
}
