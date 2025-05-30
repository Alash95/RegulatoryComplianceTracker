using System;
using System.Threading.Tasks;
using System.Text;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Azure.AI.OpenAI;
using RegulatoryComplianceTracker.Core.Interfaces;

namespace RegulatoryComplianceTracker.Infrastructure.Services
{
    public class SummarizationService : ISummarizationService
    {
        private readonly ILogger<SummarizationService> _logger;
        private readonly IConfiguration _configuration;
        private readonly OpenAIClient _openAIClient;

        public SummarizationService(ILogger<SummarizationService> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
            
            var apiKey = _configuration["OpenAI:ApiKey"];
            var endpoint = _configuration["OpenAI:Endpoint"];
            
            _openAIClient = new OpenAIClient(
                new Uri(endpoint),
                new Azure.AzureKeyCredential(apiKey));
        }

        public async Task<string> SummarizeTextAsync(string text, string documentName)
        {
            _logger.LogInformation($"Summarizing document: {documentName}");
            
            try
            {
                var deploymentName = _configuration["OpenAI:DeploymentName"];
                
                // Truncate text if it's too long (12000 chars as in the original Python code)
                var truncatedText = text.Length > 12000 ? text.Substring(0, 12000) : text;
                
                var chatCompletionsOptions = new ChatCompletionsOptions()
                {
                    DeploymentName = deploymentName,
                    Messages =
                    {
                        new ChatMessage(ChatRole.System, "You are an regulatory compliance assistant, " +
                        "your task is to read and analyse regulatory policies from the documents within this codebase, note that the documents are pdfs, take note of the topics for " +
                        "each of this and add a read more link that takes your users to the file source" +
                        " determine the level of risk for each of the summaries based on each files scraped whether it is High, Medium or Low in  this format."),
                        new ChatMessage(ChatRole.User, truncatedText)
                    }
                };

                var response = await _openAIClient.GetChatCompletionsAsync(chatCompletionsOptions);
                var summary = response.Value.Choices[0].Message.Content;
                
                return summary;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to summarize {documentName}: {ex.Message}");
                return null;
            }
        }
    }
}
