using System.Collections.Generic;
using System.Linq;

namespace Syndication.Core
{
    public class ParserProvider : IParserProvider
    {
        private readonly List<IFeedParser> _parsers = new List<IFeedParser>();

        public ParserProvider()
        {
            // Default parsers? 
            // Maybe we should let the caller register them, or FeedReader default them.
            // For convenience, let's not auto-register here to keep it clean, 
            // but FeedReader will populate default.
        }

        public void RegisterParser(IFeedParser parser)
        {
            if (parser != null && !_parsers.Contains(parser))
            {
                _parsers.Add(parser);
            }
        }

        public IFeedParser? GetParser(string contentType, string contentSnippet)
        {
            // First try to find one that says "Yes"
            return _parsers.FirstOrDefault(p => p.CanParse(contentType, contentSnippet));
        }
        
        public IEnumerable<IFeedParser> GetParsers()
        {
             return _parsers;
        }
    }
}
