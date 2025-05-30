using System.Threading.Tasks;

namespace RegulatoryComplianceTracker.Core.Interfaces
{
    public interface IPdfProcessingService
    {
        Task<byte[]> DownloadPdfAsync(string url);
        Task<string> ExtractTextFromPdfAsync(byte[] pdfContent);
        Task<string> ExtractTextFromPdfFileAsync(string filePath);
        Task SavePdfAsync(byte[] pdfContent, string filePath);
        Task SaveTextAsync(string text, string filePath);
    }
}
