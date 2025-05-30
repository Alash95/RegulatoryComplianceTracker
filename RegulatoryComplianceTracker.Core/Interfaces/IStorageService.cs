using System.Threading.Tasks;

namespace RegulatoryComplianceTracker.Core.Interfaces
{
    public interface IStorageService
    {
        Task<string> SaveFileAsync(byte[] fileContent, string fileName, string containerName);
        Task<byte[]> GetFileAsync(string filePath, string containerName);
        Task<bool> DeleteFileAsync(string filePath, string containerName);
        Task<string[]> ListFilesAsync(string containerName, string prefix = "");
    }
}
