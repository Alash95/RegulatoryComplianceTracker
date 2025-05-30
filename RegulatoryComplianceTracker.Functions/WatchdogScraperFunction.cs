using System;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using RegulatoryComplianceTracker.Core.Interfaces;

namespace RegulatoryComplianceTracker.Functions
{
    public class WatchdogScraperFunction
    {
        private readonly ILogger<WatchdogScraperFunction> _logger;
        private readonly IDocumentService _documentService;
        private readonly INewsScrapingService _newsScrapingService;

        public WatchdogScraperFunction(
            ILogger<WatchdogScraperFunction> logger,
            IDocumentService documentService,
            INewsScrapingService newsScrapingService)
        {
            _logger = logger;
            _documentService = documentService;
            _newsScrapingService = newsScrapingService;
        }

        [Function("WatchdogScraper")]
        public async Task RunAsync([TimerTrigger("0 */5 * * * *", RunOnStartup = false)] TimerInfo myTimer)
        {
            if (myTimer.IsPastDue)
            {
                _logger.LogInformation("Timer is past due!");
            }

            _logger.LogInformation("Running watchdog scraper...");

            try
            {
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

                _logger.LogInformation("Watchdog scrape complete.");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in watchdog scraper: {ex.Message}");
                throw;
            }
        }
    }
}
