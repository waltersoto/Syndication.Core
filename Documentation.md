# Syndication.Core Documentation

**Syndication.Core** is a modern, valid, and extensible .NET library for consuming syndication feeds. It supports a wide range of formats including RSS 2.0, RSS 1.0 (RDF), Atom, JSON Feed, and ActivityPub. It also provides robust networking features like feed auto-discovery and conditional GET.

## Table of Contents
1. [Quick Start](#quick-start)
2. [Supported Formats](#supported-formats)
3. [Networking & Discovery](#networking--discovery)
4. [Extensions (Podcasts & Media)](#extensions-podcasts--media)
5. [Decentralized Web (ActivityPub & WebSub)](#decentralized-web-activitypub--websub)
6. [OPML Support](#opml-support)
7. [Architecture & Extensibility](#architecture--extensibility)

---

## Quick Start

The core entry point is the `FeedReader` class.

```csharp
using Syndication.Core;

var reader = new FeedReader();
var feed = await reader.ReadUrlAsync("https://example.com/feed.xml");

Console.WriteLine($"Title: {feed.Title}");
foreach(var item in feed.Items)
{
    Console.WriteLine($"- {item.Title} ({item.Published})");
}
```

---

## Supported Formats

Syndication.Core automatically detects and parses the following formats:

| Format | Version | Parser |
|--------|---------|--------|
| **RSS** | 2.0, 0.9x | `RssParser` |
| **Atom** | 1.0 | `AtomParser` |
| **RDF** | RSS 1.0 | `Rss10Parser` |
| **JSON Feed** | 1.0, 1.1 | `JsonFeedParser` |
| **ActivityPub** | ActivityStreams 2.0 | `ActivityPubParser` |
| **HTML (Scrape)**| Microformats2, JSON-LD | `HtmlSemanticParser` |

The `Feed.Type` property indicates the detected format:
```csharp
if (feed.Type == FeedType.Json) { /* ... */ }
```

---

## Networking & Discovery

### Auto-Discovery
If you provide a URL to a website (e.g., `https://example.com`) instead of a direct feed URL, `FeedReader` will automatically scan the HTML for `<link rel="alternate">` tags and follow the first valid feed found.

```csharp
// Will automatically find and fetch the feed from the blog home page
var feed = await reader.ReadUrlAsync("https://example.com/blog"); 
```

### Conditional GET (Caching)
Save bandwidth by using ETags and Last-Modified headers. The `ReadUrlAsync` returns a `FeedResponse` object containing this metadata.

```csharp
// First request
var response = await reader.ReadUrlAsync("https://example.com/feed.xml");
var storedEtag = response.ETag;
var storedLastModified = response.LastModified;

// Later request
var update = await reader.ReadUrlAsync("https://example.com/feed.xml", 
    etag: storedEtag, 
    lastModified: storedLastModified);

if (update.NotModified)
{
    Console.WriteLine("No new content.");
}
else
{
    ProcessFeed(update.Feed);
}
```

### Compression
Gzip and Brotli compression are automatically handled by the underlying `HttpClient`.

---

## Extensions (Podcasts & Media)

Extensions map specialized metadata into the `Feed.Extensions` and `FeedItem.Extensions` dictionary.

### Podcast (iTunes)
Supports namespaced tags like `itunes:duration`, `itunes:author`, and `itunes:image`.

```csharp
using Syndication.Core.Extensions;

if (item.Extensions.TryGetValue("itunes", out object extData))
{
    var podData = extData as PodcastItemData;
    Console.WriteLine($"Duration: {podData.Duration}");
    Console.WriteLine($"Season: {podData.Season}, Episode: {podData.Episode}");
    Console.WriteLine($"Explicit: {podData.Explicit}");
}
```

### Media RSS
Supports `media:content`, `media:thumbnail` for video/image heavy feeds.

```csharp
if (item.Extensions.TryGetValue("media", out object extData))
{
    var mediaData = extData as MediaItemData;
    Console.WriteLine($"Thumbnail: {mediaData.ThumbnailUrl}");
    
    foreach(var content in mediaData.Contents)
    {
        Console.WriteLine($"Media: {content.Url} ({content.Type})");
    }
}
```

---

## Decentralized Web (ActivityPub & WebSub)

### ActivityPub
You can consume Mastodon or generic ActivityPub "Outbox" endpoints as if they were RSS feeds.

```csharp
var reader = new FeedReader();
// URL to a Mastodon user's outbox or profile (if discovery works)
var feed = await reader.ReadUrlAsync("https://mastodon.social/users/username/outbox?page=true");

if (feed.Type == FeedType.ActivityPub)
{
    Console.WriteLine("Reading from the Fediverse!");
}
```

### WebSub (PubSubHubbub)
The library provides a client to help subscribe to push updates if a feed supports WebSub.

**1. Check for Hub support**
```csharp
var feed = await reader.ReadUrlAsync("...");
if (!string.IsNullOrEmpty(feed.Hub))
{
    Console.WriteLine($"WebSub Hub found: {feed.Hub}");
}
```

**2. Subscribe**
```csharp
using Syndication.Core.WebSub;

var client = new WebSubClient();
bool success = await client.SubscribeAsync(
    hubUrl: feed.Hub,
    topicUrl: feed.FeedLink,
    callbackUrl: "https://myapp.com/webhook",
    secret: "my_secure_secret"
);
```

**3. Verify Signatures (in your Webhook Controller)**
```csharp
// Validate incoming POST request matches your secret
bool isValid = WebSubVerifier.VerifySignature(
    requestBodyString, 
    headerXHubSignature, 
    "my_secure_secret"
);
```

---

## OPML Support

Import and export lists of feeds using OPML 2.0.

### Import
```csharp
using Syndication.Core;

// From file or string
var opmlParser = new OpmlParser();
OpmlDocument doc = await opmlParser.ParseAsync(File.OpenRead("feeds.opml"));

foreach(var outline in doc.Outlines)
{
    Console.WriteLine($"Feed: {outline.Title} - {outline.XmlUrl}");
}
```

### Export
```csharp
var doc = new OpmlDocument();
doc.Title = "My Subscriptions";
doc.Outlines.Add(new OpmlOutline { Title = "Tech", XmlUrl = "..." });

var writer = new OpmlWriter();
string xml = await writer.WriteAsync(doc);
File.WriteAllText("export.opml", xml);
```

---

## Architecture & Extensibility

### IFeedParser
The library is built on a pluggable architecture. You can create your own parser by implementing `IFeedParser`.

```csharp
public class MyCustomParser : IFeedParser
{
    public bool CanParse(string contentType, string contentSnippet)
    {
        return contentSnippet.Contains("<my-custom-format>");
    }

    public Task<Feed> ParseAsync(Stream stream, CancellationToken token)
    {
        // ... parsing logic ...
        return Task.FromResult(new Feed());
    }
}
```

Register it with `FeedReader`:
```csharp
// Note: FeedReader constructor registers defaults. 
// You might need a custom ParserProvider to inject your own if you want to replace defaults,
// or just modify the source if integrating deeply.
```
*(Note based on current implementation: `FeedReader` internally registers parsers. To fully support external registration without modifying source, one would pass a custom `IParserProvider` to the `FeedReader` constructor).*
