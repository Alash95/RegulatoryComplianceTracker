using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using RegulatoryComplianceTracker.Core.Models;

namespace RegulatoryComplianceTracker.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UpdatesController : ControllerBase
    {
        private readonly ILogger<UpdatesController> _logger;

        public UpdatesController(ILogger<UpdatesController> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        public ActionResult<IEnumerable<ComplianceUpdate>> GetAllUpdates()
        {
            try
            {
                // For demonstration, return mock data
                // In a real application, this would come from a database
                var updates = GetMockUpdates();
                return Ok(updates);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting compliance updates: {ex.Message}");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("{id}")]
        public ActionResult<ComplianceUpdate> GetUpdate(Guid id)
        {
            try
            {
                var updates = GetMockUpdates();
                var update = updates.FirstOrDefault(u => u.Id == id);
                
                if (update == null)
                {
                    return NotFound();
                }
                
                return Ok(update);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting compliance update {id}: {ex.Message}");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPut("{id}/read")]
        public ActionResult MarkAsRead(Guid id)
        {
            try
            {
                // In a real application, this would update a database
                // For demonstration, we'll just return success
                return Ok(new { message = $"Update {id} marked as read" });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error marking update {id} as read: {ex.Message}");
                return StatusCode(500, "Internal server error");
            }
        }

        private List<ComplianceUpdate> GetMockUpdates()
        {
            return new List<ComplianceUpdate>
            {
                new ComplianceUpdate
                {
                    Id = Guid.NewGuid(),
                    Title = "New CBN AML/CFT Guidelines",
                    Description = "The Central Bank of Nigeria has released new Anti-Money Laundering guidelines that will take effect next month.",
                    Severity = "High",
                    Category = "AML/CFT",
                    DateIssued = DateTime.UtcNow.AddDays(-5),
                    DateAdded = DateTime.UtcNow.AddDays(-4),
                    IsRead = false
                },
                new ComplianceUpdate
                {
                    Id = Guid.NewGuid(),
                    Title = "SEC Reporting Deadline Extension",
                    Description = "The Securities and Exchange Commission has extended the deadline for quarterly compliance reports.",
                    Severity = "Medium",
                    Category = "Reporting",
                    DateIssued = DateTime.UtcNow.AddDays(-10),
                    DateAdded = DateTime.UtcNow.AddDays(-9),
                    IsRead = true
                },
                new ComplianceUpdate
                {
                    Id = Guid.NewGuid(),
                    Title = "FIRS Tax Compliance Notice",
                    Description = "The Federal Inland Revenue Service has issued a notice on tax compliance for financial institutions.",
                    Severity = "Medium",
                    Category = "Taxation",
                    DateIssued = DateTime.UtcNow.AddDays(-15),
                    DateAdded = DateTime.UtcNow.AddDays(-14),
                    IsRead = false
                }
            };
        }
    }
}
