using System.Linq;
using Xunit;
using global::Syndication.Core;

namespace SyndicationTests
{
    public class DiscoveryTests
    {
        [Fact]
        public void DiscoverFeedUrls_FindsRssAndAtom()
        {
            var html = @"
<!DOCTYPE html>
<html>
<head>
    <title>Test Page</title>
    <link rel=""alternate"" type=""application/rss+xml"" title=""RSS"" href=""/rss.xml"" />
    <link rel=""alternate"" type=""application/atom+xml"" title=""Atom"" href=""atom.xml"" />
    <link rel=""stylesheet"" href=""style.css"" />
</head>
<body>
</body>
</html>";

            var discoverer = new HtmlFeedDiscoverer();
            var urls = discoverer.DiscoverFeedUrls(html, "https://example.com/blog/").ToList();

            Assert.Equal(2, urls.Count);
            // Relative path resolution
            Assert.Contains("https://example.com/rss.xml", urls); 
            Assert.Contains("https://example.com/blog/atom.xml", urls);
        }

        [Fact]
        public void DiscoverFeedUrls_IgnoresNonFeedLinks()
        {
            var html = @"<link rel=""alternate"" type=""text/html"" href=""page.html"" />";
            var discoverer = new HtmlFeedDiscoverer();
            var urls = discoverer.DiscoverFeedUrls(html, "https://example.com");
            Assert.Empty(urls);
        }
    }
}
