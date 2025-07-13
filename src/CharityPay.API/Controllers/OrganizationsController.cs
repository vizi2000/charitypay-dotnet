using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using CharityPay.Application.Abstractions.Services;
using CharityPay.Application.DTOs.Organization;
using System.Security.Claims;

namespace CharityPay.API.Controllers;

[ApiController]
[Route("api/v1/organizations")]
[EnableRateLimiting("api")]
public class OrganizationsController : ControllerBase
{
    private readonly IOrganizationService _organizationService;
    private readonly ILogger<OrganizationsController> _logger;

    public OrganizationsController(
        IOrganizationService organizationService,
        ILogger<OrganizationsController> logger)
    {
        _organizationService = organizationService;
        _logger = logger;
    }

    /// <summary>
    /// Get list of approved organizations with pagination
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetOrganizations(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? category = null,
        [FromQuery] string? location = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
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
    [HttpGet("{id:guid}")]
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
    /// Get organization statistics
    /// </summary>
    [HttpGet("{id:guid}/stats")]
    public async Task<IActionResult> GetOrganizationStats(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            var stats = await _organizationService.GetOrganizationStatsAsync(id, cancellationToken);
            return Ok(new { success = true, data = stats });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting stats for organization {OrganizationId}", id);
            return StatusCode(500, new { success = false, message = "An error occurred while fetching statistics" });
        }
    }

    /// <summary>
    /// Search organizations by name
    /// </summary>
    [HttpGet("search")]
    public async Task<IActionResult> SearchOrganizations(
        [FromQuery] string q,
        CancellationToken cancellationToken)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(q))
            {
                return BadRequest(new { success = false, message = "Search term is required" });
            }

            var results = await _organizationService.SearchOrganizationsAsync(q, cancellationToken);
            return Ok(new { success = true, data = results });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching organizations");
            return StatusCode(500, new { success = false, message = "An error occurred while searching organizations" });
        }
    }
}

[ApiController]
[Route("api/v1/organization")]
[Authorize(Roles = "Organization")]
[EnableRateLimiting("api")]
public class OrganizationManagementController : ControllerBase
{
    private readonly IOrganizationService _organizationService;
    private readonly ILogger<OrganizationManagementController> _logger;

    public OrganizationManagementController(
        IOrganizationService organizationService,
        ILogger<OrganizationManagementController> logger)
    {
        _organizationService = organizationService;
        _logger = logger;
    }

    /// <summary>
    /// Get current organization profile
    /// </summary>
    [HttpGet("profile")]
    public async Task<IActionResult> GetProfile(CancellationToken cancellationToken)
    {
        try
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var profile = await _organizationService.GetMyOrganizationAsync(userId, cancellationToken);
            
            if (profile == null)
            {
                return NotFound(new { success = false, message = "Organization not found" });
            }
            
            return Ok(new { success = true, data = profile });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting organization profile");
            return StatusCode(500, new { success = false, message = "An error occurred while fetching profile" });
        }
    }

    /// <summary>
    /// Update organization profile
    /// </summary>
    [HttpPut("profile")]
    public async Task<IActionResult> UpdateProfile(
        [FromBody] UpdateOrganizationRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            await _organizationService.UpdateOrganizationAsync(userId, request, cancellationToken);
            return Ok(new { success = true, message = "Profile updated successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating organization profile");
            return StatusCode(500, new { success = false, message = "An error occurred while updating profile" });
        }
    }

    /// <summary>
    /// Upload organization logo
    /// </summary>
    [HttpPost("logo")]
    public async Task<IActionResult> UploadLogo(
        [FromForm] IFormFile logo,
        CancellationToken cancellationToken)
    {
        try
        {
            if (logo == null || logo.Length == 0)
            {
                return BadRequest(new { success = false, message = "No file uploaded" });
            }

            // Validate file type
            var allowedTypes = new[] { "image/jpeg", "image/jpg", "image/png", "image/webp" };
            if (!allowedTypes.Contains(logo.ContentType.ToLower()))
            {
                return BadRequest(new { success = false, message = "Invalid file type. Only JPEG, PNG, and WebP are allowed." });
            }

            // Validate file size (max 5MB)
            if (logo.Length > 5 * 1024 * 1024)
            {
                return BadRequest(new { success = false, message = "File size too large. Maximum size is 5MB." });
            }

            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            using var stream = logo.OpenReadStream();
            var logoUrl = await _organizationService.UpdateOrganizationLogoAsync(userId, stream, logo.FileName, cancellationToken);
            
            return Ok(new { success = true, data = new { logoUrl } });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading organization logo");
            return StatusCode(500, new { success = false, message = "An error occurred while uploading logo" });
        }
    }
}