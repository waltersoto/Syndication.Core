using System.Linq;
using System.Threading.Tasks;
using Xunit;
using global::Syndication.Core;
using global::Syndication.Core.Extensions;

namespace SyndicationTests
{
    public class MediaRssTests
    {
        [Fact]
        public async Task ParseAsync_MediaRss_ReturnsExtensionData()
        {
            var xml = @"<?xml version=""1.0"" encoding=""UTF-8""?>
<rss version=""2.0"" xmlns:media=""http://search.yahoo.com/mrss/"">
  <channel>
    <title>Video Feed</title>
    <item>
      <title>Big Buck Bunny</title>
      <media:title>Big Buck Bunny Movie</media:title>
      <media:thumbnail url=""http://example.com/thumb.jpg"" />
      <media:content url=""http://example.com/movie.mp4"" fileSize=""123456"" type=""video/mp4"" medium=""video"" />
    </item>
  </channel>
</rss>";

            var reader = new FeedReader();
            var feed = await reader.ReadStringAsync(xml);

            var item = feed.Items.First();
            Assert.True(item.Extensions.ContainsKey(MediaKeys.ExtensionKey));
            
            var media = item.Extensions[MediaKeys.ExtensionKey] as MediaItemData;
            Assert.NotNull(media);
            Assert.Equal("Big Buck Bunny Movie", media.Title);
            Assert.Equal("http://example.com/thumb.jpg", media.ThumbnailUrl);
            
            Assert.Single(media.Contents);
            var content = media.Contents[0];
            Assert.Equal("http://example.com/movie.mp4", content.Url);
            Assert.Equal(123456, content.FileSize);
            Assert.Equal("video", content.Medium);
        }
    }
}
