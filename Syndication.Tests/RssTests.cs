using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using global::Syndication.Core;

namespace SyndicationTests
{
    public class RssTests
    {
        [Fact]
        public async Task ParseAsync_ValidRss_ReturnsFeed()
        {
            var xml = @"<?xml version=""1.0"" encoding=""UTF-8"" ?>
<rss version=""2.0"">
<channel>
 <title>RSS Title</title>
 <description>This is an example of an RSS feed</description>
 <link>http://www.example.com/main.html</link>
 <lastBuildDate>Mon, 06 Sep 2010 00:01:00 +0000 </lastBuildDate>
 <pubDate>Sun, 06 Sep 2009 16:20:00 +0000</pubDate>
 <item>
  <title>Example entry</title>
  <description>Here is some text containing an interesting description.</description>
  <link>http://www.example.com/blog/post/1</link>
  <guid isPermaLink=""true"">7bd204c6-1655-4c27-aeee-53f933c5395f</guid>
  <pubDate>Sun, 06 Sep 2009 16:20:00 +0000</pubDate>
 </item>
</channel>
</rss>";

            var reader = new FeedReader();
            var feed = await reader.ReadStringAsync(xml);

            Assert.NotNull(feed);
            Assert.Equal(FeedType.Rss, feed.Type);
            Assert.Equal("RSS Title", feed.Title);
            Assert.Equal("This is an example of an RSS feed", feed.Description);
            Assert.Equal("http://www.example.com/main.html", feed.Link);
            Assert.Single(feed.Items);

            var item = feed.Items.First();
            Assert.Equal("Example entry", item.Title);
            Assert.Equal("7bd204c6-1655-4c27-aeee-53f933c5395f", item.Id);
            Assert.Equal(new DateTimeOffset(2009, 9, 6, 16, 20, 0, TimeSpan.Zero), item.Published);
        }
    }
}
