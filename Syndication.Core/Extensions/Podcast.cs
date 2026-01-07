using System;

namespace Syndication.Core.Extensions
{
    public static class PodcastKeys
    {
        public const string Namespace = "http://www.itunes.com/dtds/podcast-1.0.dtd";
        public const string ExtensionKey = "itunes";
    }

    public class PodcastFeedData
    {
        public string? Author { get; set; }
        public string? Subtitle { get; set; }
        public string? Summary { get; set; }
        public string? OwnerName { get; set; }
        public string? OwnerEmail { get; set; }
        public string? ImageUrl { get; set; }
        public string? Category { get; set; }
        public bool Explicit { get; set; }
    }

    public class PodcastItemData
    {
        public string? Initial_Duration { get; set; } // Keep raw string or parse? C# TimeSpan? 
        public TimeSpan? Duration { get; set; }
        public string? Subtitle { get; set; }
        public string? Summary { get; set; }
        public string? ImageUrl { get; set; }
        public bool Explicit { get; set; }
        public int? Episode { get; set; }
        public int? Season { get; set; }
        public string? EpisodeType { get; set; } // full, trailer, bonus
    }
}
