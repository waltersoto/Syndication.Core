using System.Collections.Generic;

namespace Syndication.Core
{
    public class OpmlDocument
    {
        public string Title { get; set; } = string.Empty;
        public List<OpmlOutline> Outlines { get; set; } = new List<OpmlOutline>();
    }

    public class OpmlOutline
    {
        public string Text { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string XmlUrl { get; set; } = string.Empty;
        public string HtmlUrl { get; set; } = string.Empty;
        public List<OpmlOutline> Children { get; set; } = new List<OpmlOutline>();
    }
}
