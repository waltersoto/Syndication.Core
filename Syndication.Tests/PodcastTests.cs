using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using global::Syndication.Core;
using global::Syndication.Core.Extensions;

namespace SyndicationTests
{
    public class PodcastTests
    {
        [Fact]
        public async Task ParseAsync_PodcastFeed_ReturnsExtensionString()
        {
            var xml = @"<?xml version=""1.0"" encoding=""UTF-8""?>
<rss version=""2.0"" xmlns:itunes=""http://www.itunes.com/dtds/podcast-1.0.dtd"">
  <channel>
    <title>My Podcast</title>
    <itunes:author>John Doe</itunes:author>
    <itunes:summary>A fast-paced tech podcast.</itunes:summary>
    <itunes:explicit>no</itunes:explicit>
    <item>
      <title>Episode 1</title>
      <itunes:duration>12:34</itunes:duration>
      <itunes:episode>1</itunes:episode> 
      <itunes:season>2</itunes:season>
    </item>
  </channel>
</rss>";

            var reader = new FeedReader();
            var feed = await reader.ReadStringAsync(xml);

            Assert.NotNull(feed);
            
            // Check Feed Extension
            Assert.True(feed.Extensions.ContainsKey(PodcastKeys.ExtensionKey));
            var podData = feed.Extensions[PodcastKeys.ExtensionKey] as PodcastFeedData;
            Assert.NotNull(podData);
            Assert.Equal("John Doe", podData.Author);
            Assert.Equal("A fast-paced tech podcast.", podData.Summary);
            Assert.False(podData.Explicit);

            // Check Item Extension
            var item = feed.Items.First();
            Assert.True(item.Extensions.ContainsKey(PodcastKeys.ExtensionKey));
            var itemData = item.Extensions[PodcastKeys.ExtensionKey] as PodcastItemData;
            Assert.NotNull(itemData);
            Assert.Equal("12:34", itemData.Initial_Duration);
            Assert.Equal(1, itemData.Episode);
            Assert.Equal(2, itemData.Season);
        }
    }
}
