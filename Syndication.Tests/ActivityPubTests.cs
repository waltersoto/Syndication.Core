using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using global::Syndication.Core;

namespace SyndicationTests
{
    public class ActivityPubTests
    {
        [Fact]
        public async Task ParseAsync_ActivityPubOutbox_ReturnsFeed()
        {
            var json = @"{
  ""@context"": ""https://www.w3.org/ns/activitystreams"",
  ""type"": ""OrderedCollection"",
  ""totalItems"": 1,
  ""orderedItems"": [
    {
      ""type"": ""Create"",
      ""actor"": ""https://mastodon.social/users/test"",
      ""object"": {
        ""id"": ""https://mastodon.social/users/test/statuses/12345"",
        ""type"": ""Note"",
        ""summary"": null,
        ""content"": ""<p>Hello Fediverse!</p>"",
        ""published"": ""2025-01-01T12:00:00Z"",
        ""url"": ""https://mastodon.social/@test/12345"",
        ""attributedTo"": ""https://mastodon.social/users/test""
      }
    }
  ]
}";

            var reader = new FeedReader();
            // Use ReadStringAsync but sniffing should detect AP context
            var feed = await reader.ReadStringAsync(json);

            Assert.NotNull(feed);
            Assert.Equal(FeedType.ActivityPub, feed.Type);
            Assert.Single(feed.Items);

            var item = feed.Items.First();
            Assert.Equal("Untitled Note", item.Title); // fallback title
            Assert.Equal("<p>Hello Fediverse!</p>", item.Content);
            Assert.Equal("https://mastodon.social/@test/12345", item.Link);
            Assert.Equal(new DateTimeOffset(2025, 1, 1, 12, 0, 0, TimeSpan.Zero), item.Published);
        }
    }
}
