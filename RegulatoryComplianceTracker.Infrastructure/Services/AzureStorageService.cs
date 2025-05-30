using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.Extensions.Configuration;
using RegulatoryComplianceTracker.Core.Interfaces;

namespace RegulatoryComplianceTracker.Infrastructure.Services
{
    public class AzureStorageService : IStorageService
    {
        private readonly ILogger<AzureStorageService> _logger;
        private readonly IConfiguration _configuration;
        private readonly string _connectionString;

        public AzureStorageService(ILogger<AzureStorageService> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
            _connectionString = _configuration["Azure:StorageConnectionString"];
        }

        public async Task<string> SaveFileAsync(byte[] fileContent, string fileName, string containerName)
        {
            try
            {
                var container = await GetBlobContainerClientAsync(containerName);
                var blobClient = container.GetBlobClient(fileName);

                using (var stream = new MemoryStream(fileContent))
                {
                    await blobClient.UploadAsync(stream, overwrite: true);
                }

                _logger.LogInformation($"File {fileName} saved to container {containerName}");
                return blobClient.Uri.ToString();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to save file {fileName} to container {containerName}: {ex.Message}");
                throw;
            }
        }

        public async Task<byte[]> GetFileAsync(string filePath, string containerName)
        {
            try
            {
                var container = await GetBlobContainerClientAsync(containerName);
                var blobClient = container.GetBlobClient(filePath);

                if (!await blobClient.ExistsAsync())
                {
                    _logger.LogWarning($"File {filePath} not found in container {containerName}");
                    return null;
                }

                using (var memoryStream = new MemoryStream())
                {
                    await blobClient.DownloadToAsync(memoryStream);
                    return memoryStream.ToArray();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to get file {filePath} from container {containerName}: {ex.Message}");
                throw;
            }
        }

        public async Task<bool> DeleteFileAsync(string filePath, string containerName)
        {
            try
            {
                var container = await GetBlobContainerClientAsync(containerName);
                var blobClient = container.GetBlobClient(filePath);

                if (!await blobClient.ExistsAsync())
                {
                    _logger.LogWarning($"File {filePath} not found in container {containerName}");
                    return false;
                }

                await blobClient.DeleteAsync();
                _logger.LogInformation($"File {filePath} deleted from container {containerName}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to delete file {filePath} from container {containerName}: {ex.Message}");
                throw;
            }
        }

        public async Task<string[]> ListFilesAsync(string containerName, string prefix = "")
        {
            try
            {
                var container = await GetBlobContainerClientAsync(containerName);
                var files = new List<string>();

                var resultSegment = container.GetBlobsAsync(prefix: prefix).AsPages();
                await foreach (var blobPage in resultSegment)
                {
                    foreach (var blobItem in blobPage.Values)
                    {
                        files.Add(blobItem.Name);
                    }
                }

                return files.ToArray();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to list files in container {containerName}: {ex.Message}");
                throw;
            }
        }

        private async Task<BlobContainerClient> GetBlobContainerClientAsync(string containerName)
        {
            var blobServiceClient = new BlobServiceClient(_connectionString);
            var containerClient = blobServiceClient.GetBlobContainerClient(containerName);

            await containerClient.CreateIfNotExistsAsync(PublicAccessType.None);

            return containerClient;
        }
    }
}
