using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using RegulatoryComplianceTracker.Core.Interfaces;
using RegulatoryComplianceTracker.Core.Services;
using RegulatoryComplianceTracker.Infrastructure.Services;
using System.IO;

namespace RegulatoryComplianceTracker.Functions
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var host = new HostBuilder()
                .ConfigureFunctionsWorkerDefaults()
                .ConfigureServices(services =>
                {
                    // Register HTTP client
                    services.AddHttpClient();

                    // Register application services
                    services.AddScoped<IDocumentService, DocumentService>();
                    services.AddScoped<IPdfProcessingService, PdfProcessingService>();
                    services.AddScoped<ISummarizationService, SummarizationService>();
                    services.AddScoped<INewsScrapingService, NewsScrapingService>();
                    services.AddScoped<IStorageService, AzureStorageService>();

                    // Ensure regulatory_documents directory exists
                    Directory.CreateDirectory("regulatory_documents");
                })
                .Build();

            host.Run();
        }
    }
}
