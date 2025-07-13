using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using CharityPay.Application.Abstractions.Services;
using CharityPay.Domain.Enums;

namespace CharityPay.API.Controllers;

[ApiController]
[Route("api/v1/admin")]
[Authorize(Roles = "Admin")]
[EnableRateLimiting("api")]
public class AdminController : ControllerBase
{
    private readonly IOrganizationService _organizationService;
    private readonly ILogger<AdminController> _logger;

    public AdminController(
        IOrganizationService organizationService,
        ILogger<AdminController> logger)
    {
        _organizationService = organizationService;
        _logger = logger;
    }

    /// <summary>
    /// Get all organizations with pagination
    /// </summary>
    [HttpGet("organizations")]
    public async Task<IActionResult> GetOrganizations(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? category = null,
        [FromQuery] string? location = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Note: This uses the same method as public endpoint
            // In a real implementation, admin might see all organizations including pending ones
            var result = await _organizationService.GetOrganizationsAsync(
                page, 
                pageSize, 
                category, 
                location,
                cancellationToken);
            return Ok(new { success = true, data = result });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting organizations");
            return StatusCode(500, new { success = false, message = "An error occurred while fetching organizations" });
        }
    }

    /// <summary>
    /// Get organization details by ID
    /// </summary>
    [HttpGet("organizations/{id:guid}")]
    public async Task<IActionResult> GetOrganization(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            var organization = await _organizationService.GetOrganizationByIdAsync(id, cancellationToken);
            if (organization == null)
            {
                return NotFound(new { success = false, message = "Organization not found" });
            }
            return Ok(new { success = true, data = organization });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting organization {OrganizationId}", id);
            return StatusCode(500, new { success = false, message = "An error occurred while fetching organization" });
        }
    }

    /// <summary>
    /// Get global platform statistics
    /// </summary>
    [HttpGet("stats")]
    public async Task<IActionResult> GetGlobalStats(CancellationToken cancellationToken)
    {
        try
        {
            // TODO: Implement global stats aggregation
            var stats = new
            {
                totalOrganizations = 0,
                totalPayments = 0,
                totalRevenue = 0m,
                message = "Global statistics not yet implemented"
            };

            return Ok(new { success = true, data = stats });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting global stats");
            return StatusCode(500, new { success = false, message = "An error occurred while fetching statistics" });
        }
    }
}