using System;

namespace RegulatoryComplianceTracker.Core.Models
{
    public class RegulatoryDocument
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Source { get; set; }
        public string Url { get; set; }
        public string FilePath { get; set; }
        public string TextPath { get; set; }
        public string SummaryPath { get; set; }
        public DateTime DateAdded { get; set; }
        public DateTime LastUpdated { get; set; }
    }
}
