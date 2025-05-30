using System;

namespace RegulatoryComplianceTracker.Core.Models
{
    public class NewsArticle
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public string Source { get; set; }
        public string Url { get; set; }
        public string Content { get; set; }
        public string FilePath { get; set; }
        public DateTime DateScraped { get; set; }
    }
}
