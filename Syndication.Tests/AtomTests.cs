using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using global::Syndication.Core;

namespace SyndicationTests
{
    public class AtomTests
    {
        [Fact]
        public async Task ParseAsync_ValidAtom_ReturnsFeed()
        {
            var xml = @"<?xml version=""1.0"" encoding=""utf-8""?>
<feed xmlns=""http://www.w3.org/2005/Atom"">
 <title>Example Feed</title>
 <link href=""http://example.org/"" rel=""alternate""/>
 <updated>2003-12-13T18:30:02Z</updated>
 <author>
   <name>John Doe</name>
 </author>
 <id>urn:uuid:60a76c80-d399-11d9-b93C-0003939e0af6</id>

 <entry>
   <title>Atom-Powered Robots Run Amok</title>
   <link href=""http://example.org/2003/12/13/atom03""/>
   <id>urn:uuid:1225c695-cfb8-4ebb-aaaa-80da344efa6a</id>
   <updated>2003-12-13T18:30:02Z</updated>
   <summary>Some text.</summary>
 </entry>
</feed>";

            var reader = new FeedReader();
            var feed = await reader.ReadStringAsync(xml);

            Assert.NotNull(feed);
            Assert.Equal(FeedType.Atom, feed.Type);
            Assert.Equal("Example Feed", feed.Title);
            Assert.Equal("http://example.org/", feed.Link);
            Assert.Equal(new DateTimeOffset(2003, 12, 13, 18, 30, 2, TimeSpan.Zero), feed.LastUpdated);
            Assert.Single(feed.Items);

            var item = feed.Items.First();
            Assert.Equal("Atom-Powered Robots Run Amok", item.Title);
            Assert.Equal("urn:uuid:1225c695-cfb8-4ebb-aaaa-80da344efa6a", item.Id);
            Assert.Equal(new DateTimeOffset(2003, 12, 13, 18, 30, 2, TimeSpan.Zero), item.Updated);
        }
    }
}
