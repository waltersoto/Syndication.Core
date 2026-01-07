# Syndication.Core

A modern, lightweight, and robust C# library for reading syndication feeds (RSS, Atom, JSON, ActivityPub).

## Features
- **All Formats**: Supports RSS 2.0, RSS 1.0 (RDF), Atom, and JSON Feed.
- **Decentralized**: Consumes **ActivityPub** Outboxes and supports **WebSub** verification.
- **Smart Discovery**: Automatically finds feeds in HTML pages.
- **Robust Networking**: Handles Conditional GET (ETag), Compression, and resilient parsing.
- **Extensions**: Native support for **Podcasting (iTunes)** and **Media RSS**.

## Usage

```csharp
var reader = new FeedReader();
// Auto-discovers feed from blog URL
var response = await reader.ReadUrlAsync("https://example.com/blog");

if (response.Feed != null) 
{
    Console.WriteLine($"Title: {response.Feed.Title} ({response.Feed.Type})");
    foreach(var item in response.Feed.Items)
    {
        Console.WriteLine($"- {item.Title}");
    }
}
```

## Supported Formats
- RSS 2.0 & 1.0 (RDF)
- Atom 1.0
- JSON Feed 1.0 & 1.1
- ActivityPub (ActivityStreams 2.0)
- OPML 2.0 (Import/Export)

## Documentation
For full usage details, architecture explanation, and examples, see the [Comprehensive Documentation](Documentation.md).

## License
MIT
