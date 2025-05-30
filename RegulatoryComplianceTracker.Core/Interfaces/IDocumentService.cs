using System.Threading.Tasks;
using RegulatoryComplianceTracker.Core.Models;

namespace RegulatoryComplianceTracker.Core.Interfaces
{
    public interface IDocumentService
    {
        Task<RegulatoryDocument[]> GetAllDocumentsAsync();
        Task<RegulatoryDocument> GetDocumentByIdAsync(string id);
        Task<string> GetDocumentTextAsync(string id);
        Task<string> GetDocumentSummaryAsync(string id);
        Task<byte[]> DownloadDocumentAsync(string id);
        Task<RegulatoryDocument> AddDocumentAsync(string name, string url);
        Task UpdateDocumentAsync(RegulatoryDocument document);
        Task<RegulatoryDocument> GetDocumentByIdApiAsync(string id);
    }
}
