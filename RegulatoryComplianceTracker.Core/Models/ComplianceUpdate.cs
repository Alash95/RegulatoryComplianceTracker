using System;

namespace RegulatoryComplianceTracker.Core.Models
{
    public class ComplianceUpdate
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Severity { get; set; }
        public string Category { get; set; }
        public DateTime DateIssued { get; set; }
        public DateTime DateAdded { get; set; }
        public bool IsRead { get; set; }
    }
}
