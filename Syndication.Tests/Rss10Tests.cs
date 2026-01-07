using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using global::Syndication.Core;

namespace SyndicationTests
{
    public class Rss10Tests
    {
        [Fact]
        public async Task ParseAsync_ValidRss10_ReturnsFeed()
        {
            var xml = @"<?xml version=""1.0""?>
<rdf:RDF 
  xmlns:rdf=""http://www.w3.org/1999/02/22-rdf-syntax-ns#""
  xmlns=""http://purl.org/rss/1.0/""
  xmlns:dc=""http://purl.org/dc/elements/1.1/""
>
  <channel rdf:about=""http://www.xml.com/xml/news.rss"">
    <title>XML.com</title>
    <link>http://xml.com/pub</link>
    <description>XML.com features a rich mix of information and services for the XML community.</description>
    <image rdf:resource=""http://xml.com/universal/images/xml_tiny.gif"" />
    <items>
      <rdf:Seq>
        <rdf:li rdf:resource=""http://xml.com/pub/2000/08/09/xslt/xslt.html"" />
        <rdf:li rdf:resource=""http://xml.com/pub/2000/08/09/rdfdb/index.html"" />
      </rdf:Seq>
    </items>
  </channel>
  <image rdf:about=""http://xml.com/universal/images/xml_tiny.gif"">
    <url>http://xml.com/universal/images/xml_tiny.gif</url>
    <title>XML.com</title>
    <link>http://www.xml.com</link>
  </image>
  <item rdf:about=""http://xml.com/pub/2000/08/09/xslt/xslt.html"">
    <title>Processing Inclusions with XSLT</title>
    <link>http://xml.com/pub/2000/08/09/xslt/xslt.html</link>
    <description>Processing Inclusions with XSLT</description>
    <dc:date>2000-08-09T07:00:00Z</dc:date>
  </item>
</rdf:RDF>";

            var reader = new FeedReader();
            var feed = await reader.ReadStringAsync(xml);

            Assert.NotNull(feed);
            Assert.Equal(FeedType.Rss, feed.Type);
            Assert.Equal("XML.com", feed.Title);
            Assert.Equal("http://xml.com/pub", feed.Link);
            Assert.Single(feed.Items);

            var item = feed.Items.First();
            Assert.Equal("Processing Inclusions with XSLT", item.Title);
            Assert.Equal(new DateTimeOffset(2000, 8, 9, 7, 0, 0, TimeSpan.Zero), item.Published);
        }
    }
}
