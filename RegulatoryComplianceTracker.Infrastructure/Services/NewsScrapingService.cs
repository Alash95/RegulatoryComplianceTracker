using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using HtmlAgilityPack;
using RegulatoryComplianceTracker.Core.Interfaces;
using RegulatoryComplianceTracker.Core.Models;

namespace RegulatoryComplianceTracker.Infrastructure.Services
{
    public class NewsScrapingService : INewsScrapingService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<NewsScrapingService> _logger;
        private readonly string _outputDir;

        public NewsScrapingService(HttpClient httpClient, ILogger<NewsScrapingService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
            _outputDir = "regulatory_documents";
            
            // Ensure the directory exists
            System.IO.Directory.CreateDirectory(_outputDir);
        }

        public async Task<IEnumerable<NewsArticle>> ScrapeNewsArticlesAsync(string url, int maxArticles = 5)
        {
            _logger.LogInformation($"Scraping news articles from: {url}");
            var articles = new List<NewsArticle>();
            
            try
            {
                var response = await _httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();
                
                var htmlContent = await response.Content.ReadAsStringAsync();
                var htmlDocument = new HtmlDocument();
                htmlDocument.LoadHtml(htmlContent);
                
                var articleNodes = htmlDocument.DocumentNode.SelectNodes("//article");
                
                if (articleNodes == null || articleNodes.Count == 0)
                {
                    _logger.LogWarning("No articles found.");
                    return articles;
                }
                
                int count = 0;
                foreach (var articleNode in articleNodes)
                {
                    if (count >= maxArticles)
                        break;
                    
                    var titleNode = articleNode.SelectSingleNode(".//h2");
                    if (titleNode == null)
                        continue;
                    
                    var linkNode = articleNode.SelectSingleNode(".//a");
                    if (linkNode == null)
                        continue;
                    
                    var title = titleNode.InnerText.Trim();
                    var link = linkNode.GetAttributeValue("href", "");
                    
                    if (string.IsNullOrEmpty(title) || string.IsNullOrEmpty(link))
                        continue;
                    
                    _logger.LogInformation($"Article: {title} - {link}");
                    
                    var article = await ScrapeArticleContentAsync(link, title);
                    if (article != null)
                    {
                        articles.Add(article);
                        count++;
                    }
                }
                
                return articles;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to scrape news from {url}: {ex.Message}");
                return articles;
            }
        }

        public async Task<NewsArticle> ScrapeArticleContentAsync(string url, string title)
        {
            try
            {
                var response = await _httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();
                
                var htmlContent = await response.Content.ReadAsStringAsync();
                var htmlDocument = new HtmlDocument();
                htmlDocument.LoadHtml(htmlContent);
                
                var contentNodes = htmlDocument.DocumentNode.SelectNodes("//p");
                if (contentNodes == null || contentNodes.Count == 0)
                {
                    _logger.LogWarning($"No content found for article: {title}");
                    return null;
                }
                
                var contentBuilder = new System.Text.StringBuilder();
                foreach (var node in contentNodes)
                {
                    contentBuilder.AppendLine(node.InnerText.Trim());
                }
                
                var content = contentBuilder.ToString();
                
                var article = new NewsArticle
                {
                    Id = Guid.NewGuid(),
                    Title = title,
                    Source = "Punch",
                    Url = url,
                    Content = content,
                    DateScraped = DateTime.UtcNow
                };
                
                return article;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to scrape article content from {url}: {ex.Message}");
                return null;
            }
        }

        public async Task SaveArticleAsync(NewsArticle article)
        {
            try
            {
                // Create a safe filename from the title
                var safeTitle = Regex.Replace(article.Title, @"[^\w\s]", "");
                safeTitle = Regex.Replace(safeTitle, @"\s+", "_");
                if (safeTitle.Length > 50)
                    safeTitle = safeTitle.Substring(0, 50);
                
                var fileName = $"Punch_{DateTime.UtcNow:yyyyMMdd}_{safeTitle}.txt";
                var filePath = System.IO.Path.Combine(_outputDir, fileName);
                
                var content = $"{article.Title}\n{article.Url}\n\n{article.Content}";
                
                await System.IO.File.WriteAllTextAsync(filePath, content);
                
                // Update the article with the file path
                article.FilePath = filePath;
                
                _logger.LogInformation($"Saved article: {fileName}");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to save article {article.Title}: {ex.Message}");
                throw;
            }
        }
    }
}
