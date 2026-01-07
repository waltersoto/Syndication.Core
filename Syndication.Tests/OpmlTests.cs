using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using global::Syndication.Core;

namespace SyndicationTests
{
    public class OpmlTests
    {
        [Fact]
        public async Task ParseAsync_ValidOpml_ReturnsDocument()
        {
            var xml = @"<?xml version=""1.0"" encoding=""ISO-8859-1""?>
<opml version=""2.0"">
	<head>
		<title>mySubscriptions.opml</title>
		<dateCreated>Sat, 18 Jun 2005 12:11:52 GMT</dateCreated>
	</head>
	<body>
		<outline text=""CNET News.com"" description=""Tech news"" htmlUrl=""http://news.com.com/"" language=""unknown"" title=""CNET News.com"" type=""rss"" version=""RSS2"" xmlUrl=""http://news.com.com/2547-1_3-0-5.xml""/>
        <outline text=""Sub Folder"">
            <outline text=""Nested Feed"" xmlUrl=""http://nested.com/feed"" />
        </outline>
	</body>
</opml>";

            var parser = new OpmlParser();
            using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(xml)))
            {
                var doc = await parser.ParseAsync(stream);

                Assert.NotNull(doc);
                Assert.Equal("mySubscriptions.opml", doc.Title);
                Assert.Equal(2, doc.Outlines.Count);
                
                var first = doc.Outlines[0];
                Assert.Equal("CNET News.com", first.Text);
                Assert.Equal("http://news.com.com/2547-1_3-0-5.xml", first.XmlUrl);
                
                var folder = doc.Outlines[1];
                Assert.Equal("Sub Folder", folder.Text);
                Assert.Single(folder.Children);
                Assert.Equal("Nested Feed", folder.Children[0].Text);
            }
        }
    }
}
