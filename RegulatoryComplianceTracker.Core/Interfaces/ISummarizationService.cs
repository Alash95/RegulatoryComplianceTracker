using System.Threading.Tasks;

namespace RegulatoryComplianceTracker.Core.Interfaces
{
    public interface ISummarizationService
    {
        Task<string> SummarizeTextAsync(string text, string documentName);
    }
}
