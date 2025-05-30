using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.IO;
using System.Text;
using Microsoft.Extensions.Logging;
using RegulatoryComplianceTracker.Core.Interfaces;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas.Parser;
using iText.Kernel.Pdf.Canvas.Parser.Listener;

namespace RegulatoryComplianceTracker.Infrastructure.Services
{
    public class PdfProcessingService : IPdfProcessingService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<PdfProcessingService> _logger;

        public PdfProcessingService(HttpClient httpClient, ILogger<PdfProcessingService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<byte[]> DownloadPdfAsync(string url)
        {
            _logger.LogInformation($"Downloading PDF from: {url}");
            try
            {
                var response = await _httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadAsByteArrayAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to download PDF from {url}: {ex.Message}");
                throw;
            }
        }

        public async Task<string> ExtractTextFromPdfAsync(byte[] pdfContent)
        {
            _logger.LogInformation("Extracting text from PDF content using iText 7");
            try
            {
                var textBuilder = new StringBuilder();
                using (var memoryStream = new MemoryStream(pdfContent))
                using (var pdfReader = new PdfReader(memoryStream))
                using (var pdfDocument = new PdfDocument(pdfReader))
                {
                    for (int i = 1; i <= pdfDocument.GetNumberOfPages(); i++)
                    {
                        var strategy = new LocationTextExtractionStrategy();
                        var pageText = PdfTextExtractor.GetTextFromPage(pdfDocument.GetPage(i), strategy);
                        textBuilder.Append(pageText);
                    }
                }
                return await Task.FromResult(textBuilder.ToString()); // Wrap in Task.FromResult for async signature
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to extract text from PDF using iText 7: {ex.Message}");
                throw;
            }
        }

        public async Task<string> ExtractTextFromPdfFileAsync(string filePath)
        {
            _logger.LogInformation($"Extracting text from PDF file: {filePath}");
            try
            {
                var pdfBytes = await File.ReadAllBytesAsync(filePath);
                return await ExtractTextFromPdfAsync(pdfBytes);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to extract text from PDF file {filePath}: {ex.Message}");
                throw;
            }
        }

        public async Task SavePdfAsync(byte[] pdfContent, string filePath)
        {
            _logger.LogInformation($"Saving PDF to: {filePath}");
            try
            {
                var directory = Path.GetDirectoryName(filePath);
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }
                
                await File.WriteAllBytesAsync(filePath, pdfContent);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to save PDF to {filePath}: {ex.Message}");
                throw;
            }
        }

        public async Task SaveTextAsync(string text, string filePath)
        {
            _logger.LogInformation($"Saving text to: {filePath}");
            try
            {
                var directory = Path.GetDirectoryName(filePath);
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }
                
                await File.WriteAllTextAsync(filePath, text);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to save text to {filePath}: {ex.Message}");
                throw;
            }
        }
    }
}
