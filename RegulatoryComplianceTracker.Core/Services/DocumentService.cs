using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Unicode;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using RegulatoryComplianceTracker.Core.Interfaces;
using RegulatoryComplianceTracker.Core.Models;

namespace RegulatoryComplianceTracker.Core.Services
{
    public class DocumentService : IDocumentService
    {
        private readonly IPdfProcessingService _pdfProcessingService;
        private readonly ISummarizationService _summarizationService;
        private readonly IStorageService _storageService;
        private readonly ILogger<DocumentService> _logger;
        private readonly Dictionary<string, string> _pdfLinks;
        private readonly string _outputDir;

        public DocumentService(
            IPdfProcessingService pdfProcessingService,
            ISummarizationService summarizationService,
            IStorageService storageService,
            ILogger<DocumentService> logger)
        {
            _pdfProcessingService = pdfProcessingService;
            _summarizationService = summarizationService;
            _storageService = storageService;
            _logger = logger;
            _outputDir = "regulatory_documents";
            
            // Initialize PDF sources (same as in the original Python code)
            _pdfLinks = new Dictionary<string, string>
            {
                { "CBN_Customer_Due_Diligence_2023", "https://www.cbn.gov.ng/Out/2023/CCD/CBN%20Customer%20Due%20diligence%20Reg.%202023-combined.pdf" },
                { "CBN_AML_CFT_2022", "https://www.cbn.gov.ng/Out/2022/FPRD/AML%20CIRCULAR%20AND%20REGULATIONS%20MERGED.pdf" },
                { "SEC_AML_CFT_Regulations_2022", "https://sec.gov.ng/wp-content/uploads/2022/10/SEC-AMLCFTCPF-REGULATIONS-12-MAY-2022.pdf" },
                { "SEC_AML_CFT_Manual", "https://sec.gov.ng/files/AML%20CFT%20COMPLIANCE%20MANUAL.pdf" },
                { "FIRS_Common_Reporting_Regulations", "https://old.firs.gov.ng/wp-content/uploads/2020/10/6f675124-b234-4482-ef83-0ef3e1e6d10fINCOME-TAX-COMMON-REPORTING-STANDARD.pdf" }
            };
            
            // Ensure the output directory exists
            System.IO.Directory.CreateDirectory(_outputDir);
        }

        public async Task<RegulatoryDocument[]>GetAllDocumentsAsync()
        {
            var documents = new List<RegulatoryDocument>();

            foreach (var pair in _pdfLinks)
            {
                var document = await GetDocumentByIdAsync(pair.Key);
                if (document != null)
                {
                    documents.Add(document);
                }
            }

            return documents.ToArray();
        }

        public async Task<RegulatoryDocument> GetDocumentByIdAsync(string id)
        {
            if (!_pdfLinks.ContainsKey(id))
            {
                _logger.LogWarning($"Document with ID {id} not found");
                return null;
            }
            
            var url = _pdfLinks[id];
            var pdfPath = System.IO.Path.Combine(_outputDir, $"{id}.pdf");
            var textPath = System.IO.Path.Combine(_outputDir, $"{id}.txt");
            var summaryPath = System.IO.Path.Combine(_outputDir, $"{id}_SUMMARY.txt");
            
            // Check if the document exists
            if (!System.IO.File.Exists(pdfPath))
            {
                // If not, try to download it
                await DownloadAndProcessDocumentAsync(id, url);
            }

            //Guid.TryParse(id.GetHashCode().ToString(), out var x);
            // Create and return the document object
            var idByte = Encoding.UTF8.GetBytes(id);
            return new RegulatoryDocument
            {
                //Id = Guid.Parse(id.GetHashCode().ToString("x8") + "0000000000000000"),
                Id = Convert.ToBase64String(idByte),
                Name = id,
                Source = GetSourceFromId(id),
                Url = url,
                FilePath = pdfPath,
                TextPath = textPath,
                SummaryPath = summaryPath,
                DateAdded = System.IO.File.GetCreationTime(pdfPath),
                LastUpdated = System.IO.File.GetLastWriteTime(pdfPath)
            };
        }

        public async Task<RegulatoryDocument> GetDocumentByIdApiAsync(string id)
        {
            //Guid.TryParse(id, out var guid);

            var documents = await GetAllDocumentsAsync();
            if (!documents.Any(x => x.Id == id))
            {
                _logger.LogWarning($"Document with ID {id} not found");
                return null;
            }

            var document = documents.FirstOrDefault(x => x.Id == id);
            var url = _pdfLinks[document.Name];
            var pdfPath = System.IO.Path.Combine(_outputDir, $"{document.Name}.pdf");
            var textPath = System.IO.Path.Combine(_outputDir, $"{document.Name}.txt");
            var summaryPath = System.IO.Path.Combine(_outputDir, $"{document.Name}_SUMMARY.txt");

            //// Check if the document exists
            //if (!System.IO.File.Exists(pdfPath))
            //{
            //    // If not, try to download it
            //    await DownloadAndProcessDocumentAsync(id, url);
            //}

            ////Guid.TryParse(id.GetHashCode().ToString(), out var x);
            //// Create and return the document object
            //var idByte = Encoding.UTF8.GetBytes(id);
            //return new RegulatoryDocument
            //{
            //    //Id = Guid.Parse(id.GetHashCode().ToString("x8") + "0000000000000000"),
            //    Id = Convert.ToBase64String(idByte),
            //    Name = id,
            //    Source = GetSourceFromId(id),
            //    Url = url,
            //    FilePath = pdfPath,
            //    TextPath = textPath,
            //    SummaryPath = summaryPath,
            //    DateAdded = System.IO.File.GetCreationTime(pdfPath),
            //    LastUpdated = System.IO.File.GetLastWriteTime(pdfPath)
            //};
            return document;
        }

        public async Task<string> GetDocumentTextAsync(string id)
        {
            var document = await GetDocumentByIdAsync(id);
            if (document == null)
            {
                return null;
            }
            
            if (!System.IO.File.Exists(document.TextPath))
            {
                _logger.LogWarning($"Text file for document {id} not found");
                return null;
            }
            
            return await System.IO.File.ReadAllTextAsync(document.TextPath);
        }

        public async Task<string> GetDocumentSummaryAsync(string id)
        {
            var document = await GetDocumentByIdAsync(id);
            if (document == null)
            {
                return null;
            }
            
            if (!System.IO.File.Exists(document.SummaryPath))
            {
                _logger.LogWarning($"Summary file for document {id} not found");
                return null;
            }
            
            return await System.IO.File.ReadAllTextAsync(document.SummaryPath);
        }

        public async Task<byte[]> DownloadDocumentAsync(string id)
        {
            var document = await GetDocumentByIdAsync(id);
            if (document == null)
            {
                return null;
            }
            
            if (!System.IO.File.Exists(document.FilePath))
            {
                _logger.LogWarning($"PDF file for document {id} not found");
                return null;
            }
            
            return await System.IO.File.ReadAllBytesAsync(document.FilePath);
        }

        public async Task<RegulatoryDocument> AddDocumentAsync(string name, string url)
        {
            if (_pdfLinks.ContainsKey(name))
            {
                _logger.LogWarning($"Document with name {name} already exists");
                return await GetDocumentByIdAsync(name);
            }
            
            _pdfLinks[name] = url;
            
            // Download and process the document
            await DownloadAndProcessDocumentAsync(name, url);
            
            return await GetDocumentByIdAsync(name);
        }

        public async Task UpdateDocumentAsync(RegulatoryDocument document)
        {
            if (!_pdfLinks.ContainsKey(document.Name))
            {
                _logger.LogWarning($"Document with name {document.Name} not found");
                return;
            }
            
            _pdfLinks[document.Name] = document.Url;
            
            // Re-download and process the document
            await DownloadAndProcessDocumentAsync(document.Name, document.Url);
        }

        private async Task DownloadAndProcessDocumentAsync(string name, string url)
        {
            _logger.LogInformation($"Downloading and processing document: {name}");
            
            try
            {
                var pdfPath = System.IO.Path.Combine(_outputDir, $"{name}.pdf");
                var textPath = System.IO.Path.Combine(_outputDir, $"{name}.txt");
                var summaryPath = System.IO.Path.Combine(_outputDir, $"{name}_SUMMARY.txt");
                
                // Download the PDF
                var pdfContent = await _pdfProcessingService.DownloadPdfAsync(url);
                
                // Save the PDF
                await _pdfProcessingService.SavePdfAsync(pdfContent, pdfPath);
                
                // Extract text from the PDF
                var text = await _pdfProcessingService.ExtractTextFromPdfAsync(pdfContent);
                
                if (string.IsNullOrWhiteSpace(text))
                {
                    _logger.LogWarning($"No extractable text in {name}");
                }
                
                // Save the extracted text
                await _pdfProcessingService.SaveTextAsync(text, textPath);
                _logger.LogInformation($"Saved raw text: {textPath}");
                
                // Summarize the text
                var summary = await _summarizationService.SummarizeTextAsync(text, name);
                
                if (!string.IsNullOrWhiteSpace(summary))
                {
                    // Save the summary
                    await _pdfProcessingService.SaveTextAsync(summary, summaryPath);
                    _logger.LogInformation($"Saved summary: {summaryPath}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to download or process {name}: {ex.Message}");
                throw;
            }
        }

        private string GetSourceFromId(string id)
        {
            if (id.StartsWith("CBN"))
                return "Central Bank of Nigeria";
            else if (id.StartsWith("SEC"))
                return "Securities and Exchange Commission";
            else if (id.StartsWith("FIRS"))
                return "Federal Inland Revenue Service";
            else
                return "Unknown";
        }
    }
}
