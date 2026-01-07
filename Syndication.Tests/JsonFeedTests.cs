using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using global::Syndication.Core;

namespace SyndicationTests
{
    public class JsonFeedTests
    {
        [Fact]
        public async Task ParseAsync_ValidJson_ReturnsFeed()
        {
            var json = @"{
    ""version"": ""https://jsonfeed.org/version/1"",
    ""title"": ""My Example Feed"",
    ""home_page_url"": ""https://example.org/"",
    ""feed_url"": ""https://example.org/feed.json"",
    ""items"": [
        {
            ""id"": ""2"",
            ""content_text"": ""This is a second item."",
            ""url"": ""https://example.org/second-item""
        },
        {
            ""id"": ""1"",
            ""content_html"": ""<p>Hello, world!</p>"",
            ""url"": ""https://example.org/initial-post"",
            ""date_published"": ""2025-01-01T12:00:00Z""
        }
    ]
}";

            var reader = new FeedReader();
            // Test detection by content snippet (no content type)
            var feed = await reader.ReadStringAsync(json);

            Assert.NotNull(feed);
            Assert.Equal(FeedType.Json, feed.Type);
            Assert.Equal("My Example Feed", feed.Title);
            Assert.Equal("https://example.org/", feed.Link);
            Assert.Equal(2, feed.Items.Count);

            var item1 = feed.Items.First(i => i.Id == "1");
            Assert.Equal("<p>Hello, world!</p>", item1.Content);
            Assert.Equal(new DateTimeOffset(2025, 1, 1, 12, 0, 0, TimeSpan.Zero), item1.Published);
        }
    }
}
