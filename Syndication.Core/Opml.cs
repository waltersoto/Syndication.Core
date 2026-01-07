using System.Collections.Generic;

namespace Syndication.Core
{
    public class OpmlDocument
    {
        public string Title { get; set; }
        public List<OpmlOutline> Outlines { get; set; } = new List<OpmlOutline>();
    }

    public class OpmlOutline
    {
        public string Text { get; set; }
        public string Type { get; set; }
        public string XmlUrl { get; set; }
        public string HtmlUrl { get; set; }
        public List<OpmlOutline> Children { get; set; } = new List<OpmlOutline>();
    }
}
