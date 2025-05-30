using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using RegulatoryComplianceTracker.Core.Interfaces;

namespace RegulatoryComplianceTracker.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AdminController : ControllerBase
    {
        private readonly ILogger<AdminController> _logger;
        private readonly IDocumentService _documentService;
        private readonly INewsScrapingService _newsScrapingService;

        public AdminController(
            ILogger<AdminController> logger,
            IDocumentService documentService,
            INewsScrapingService newsScrapingService)
        {
            _logger = logger;
            _documentService = documentService;
            _newsScrapingService = newsScrapingService;
        }

        [HttpPost("trigger-scrape")]
        public async Task<ActionResult> TriggerScrape()
        {
            try
            {
                _logger.LogInformation("Manually triggering scraping process...");
                
                // Process all regulatory documents
                var documents = await _documentService.GetAllDocumentsAsync();
                foreach (var document in documents)
                {
                    await _documentService.UpdateDocumentAsync(document);
                }

                // Scrape news articles
                var newsUrl = "https://punchng.com/topics/money-laundering/";
                var articles = await _newsScrapingService.ScrapeNewsArticlesAsync(newsUrl, 5);
                foreach (var article in articles)
                {
                    await _newsScrapingService.SaveArticleAsync(article);
                }

                return Ok(new { message = "Scraping process completed successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error triggering scrape: {ex.Message}");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("scrape-status")]
        public ActionResult GetScrapeStatus()
        {
            try
            {
                // In a real application, this would check the status of background jobs
                // For demonstration, we'll just return a mock status
                return Ok(new { status = "No active scraping jobs", lastRun = DateTime.UtcNow.AddHours(-1) });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting scrape status: {ex.Message}");
                return StatusCode(500, "Internal server error");
            }
        }
    }
}
