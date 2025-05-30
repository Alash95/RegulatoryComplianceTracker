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
    public class NewsController : ControllerBase
    {
        private readonly INewsScrapingService _newsScrapingService;
        private readonly ILogger<NewsController> _logger;

        public NewsController(INewsScrapingService newsScrapingService, ILogger<NewsController> logger)
        {
            _newsScrapingService = newsScrapingService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<NewsArticle>>> GetAllNews()
        {
            try
            {
                // For demonstration, we'll scrape news on demand
                // In a real application, this would come from a database
                var newsUrl = "https://punchng.com/topics/money-laundering/";
                var articles = await _newsScrapingService.ScrapeNewsArticlesAsync(newsUrl, 5);
                
                return Ok(articles);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting news articles: {ex.Message}");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("{id}")]
        public ActionResult<NewsArticle> GetNewsArticle(Guid id)
        {
            try
            {
                // In a real application, this would fetch from a database
                // For demonstration, we'll return a not found since we don't have persistent storage
                return NotFound("Article retrieval by ID requires database implementation");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting news article {id}: {ex.Message}");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("latest")]
        public async Task<ActionResult<IEnumerable<NewsArticle>>> GetLatestNews()
        {
            try
            {
                // For demonstration, we'll scrape news on demand
                var newsUrl = "https://punchng.com/topics/money-laundering/";
                var articles = await _newsScrapingService.ScrapeNewsArticlesAsync(newsUrl, 3);
                
                return Ok(articles);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting latest news: {ex.Message}");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPost("scrape")]
        public async Task<ActionResult> ScrapeNews([FromBody] ScrapeNewsRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.Url))
                {
                    return BadRequest("URL is required");
                }

                var articles = await _newsScrapingService.ScrapeNewsArticlesAsync(request.Url, request.MaxArticles ?? 5);
                foreach (var article in articles)
                {
                    await _newsScrapingService.SaveArticleAsync(article);
                }

                return Ok(new { message = $"Successfully scraped {articles.Count()} articles" });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error scraping news: {ex.Message}");
                return StatusCode(500, "Internal server error");
            }
        }
    }

    public class ScrapeNewsRequest
    {
        public string Url { get; set; }
        public int? MaxArticles { get; set; }
    }
}
