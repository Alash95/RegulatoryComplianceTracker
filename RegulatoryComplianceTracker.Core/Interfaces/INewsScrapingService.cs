using System.Threading.Tasks;
using System.Collections.Generic;
using RegulatoryComplianceTracker.Core.Models;

namespace RegulatoryComplianceTracker.Core.Interfaces
{
    public interface INewsScrapingService
    {
        Task<IEnumerable<NewsArticle>> ScrapeNewsArticlesAsync(string url, int maxArticles = 5);
        Task<NewsArticle> ScrapeArticleContentAsync(string url, string title);
        Task SaveArticleAsync(NewsArticle article);
    }
}
