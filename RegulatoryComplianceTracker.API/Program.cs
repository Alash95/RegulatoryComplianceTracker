using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using RegulatoryComplianceTracker.Core.Interfaces;
using RegulatoryComplianceTracker.Core.Services;
using RegulatoryComplianceTracker.Infrastructure.Services;

namespace RegulatoryComplianceTracker.API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container
            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Regulatory Compliance Tracker API", Version = "v1" });
            });

            // Configure CORS for frontend integration
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowSvelteApp", policy =>
                {
                    policy.AllowAnyOrigin()
                          .AllowAnyMethod()
                          .AllowAnyHeader();
                });
            });

            // Register HTTP client
            builder.Services.AddHttpClient();

            // Register application services
            builder.Services.AddScoped<IDocumentService, DocumentService>();
            builder.Services.AddScoped<IPdfProcessingService, PdfProcessingService>();
            builder.Services.AddScoped<ISummarizationService, SummarizationService>();
            builder.Services.AddScoped<INewsScrapingService, NewsScrapingService>();
            builder.Services.AddScoped<IStorageService, AzureStorageService>();

            // Ensure regulatory_documents directory exists
            Directory.CreateDirectory("regulatory_documents");

            var app = builder.Build();

            // Configure the HTTP request pipeline

            app.UseSwagger();
            app.UseSwaggerUI();


            app.UseHttpsRedirection();
            app.UseAuthorization();
            app.UseCors("AllowSvelteApp");
            app.MapControllers();

            app.Run();
        }
    }
}
