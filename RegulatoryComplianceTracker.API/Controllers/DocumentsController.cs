using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using RegulatoryComplianceTracker.Core.Interfaces;
using RegulatoryComplianceTracker.Core.Models;

namespace RegulatoryComplianceTracker.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DocumentsController : ControllerBase
    {
        private readonly IDocumentService _documentService;
        private readonly ILogger<DocumentsController> _logger;

        public DocumentsController(IDocumentService documentService, ILogger<DocumentsController> logger)
        {
            _documentService = documentService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<RegulatoryDocument>>> GetAllDocuments()
        {
            try
            {
                var documents = await _documentService.GetAllDocumentsAsync();
                return Ok(documents);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting all documents: {ex.Message}");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<RegulatoryDocument>> GetDocument(string id)
        {
            try
            {
                var document = await _documentService.GetDocumentByIdApiAsync(id);
                if (document == null)
                {
                    return NotFound();
                }
                return Ok(document);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting document {id}: {ex.Message}");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("{id}/text")]
        public async Task<ActionResult<string>> GetDocumentText(string id)
        {
            try
            {
                var text = await _documentService.GetDocumentTextAsync(id);
                if (text == null)
                {
                    return NotFound();
                }
                return Ok(text);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting document text for {id}: {ex.Message}");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("{id}/summary")]
        public async Task<ActionResult<string>> GetDocumentSummary(string id)
        {
            try
            {
                var summary = await _documentService.GetDocumentSummaryAsync(id);
                if (summary == null)
                {
                    return NotFound();
                }
                return Ok(summary);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting document summary for {id}: {ex.Message}");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("download/{id}")]
        public async Task<ActionResult> DownloadDocument(string id)
        {
            try
            {
                var document = await _documentService.GetDocumentByIdAsync(id);
                if (document == null)
                {
                    return NotFound();
                }

                var pdfBytes = await _documentService.DownloadDocumentAsync(id);
                if (pdfBytes == null)
                {
                    return NotFound();
                }

                return File(pdfBytes, "application/pdf", $"{document.Name}.pdf");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error downloading document {id}: {ex.Message}");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPost]
        public async Task<ActionResult<RegulatoryDocument>> AddDocument([FromBody] AddDocumentRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.Name) || string.IsNullOrEmpty(request.Url))
                {
                    return BadRequest("Name and URL are required");
                }

                var document = await _documentService.AddDocumentAsync(request.Name, request.Url);
                return CreatedAtAction(nameof(GetDocument), new { id = document.Name }, document);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error adding document: {ex.Message}");
                return StatusCode(500, "Internal server error");
            }
        }
    }

    public class AddDocumentRequest
    {
        public string Name { get; set; }
        public string Url { get; set; }
    }
}
