using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CharityPay.Application.Services;
using CharityPay.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace CharityPay.API.Controllers;

[ApiController]
[Route("api/v1/merchant")]
[Authorize]
public class MerchantOnboardingController : ControllerBase
{
    private readonly IMerchantOnboardingService _merchantOnboardingService;
    private readonly ILogger<MerchantOnboardingController> _logger;

    public MerchantOnboardingController(
        IMerchantOnboardingService merchantOnboardingService,
        ILogger<MerchantOnboardingController> logger)
    {
        _merchantOnboardingService = merchantOnboardingService;
        _logger = logger;
    }

    /// <summary>
    /// Initiates merchant registration with Polcard
    /// </summary>
    [HttpPost("register")]
    public async Task<IActionResult> RegisterMerchant([FromBody] RegisterMerchantRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var organizationId = GetOrganizationIdFromClaims();
            
            var merchantId = await _merchantOnboardingService.InitiateMerchantRegistrationAsync(
                organizationId,
                request.LegalBusinessName,
                request.TaxId,
                request.KrsNumber,
                request.BankAccount,
                cancellationToken);

            _logger.LogInformation("Merchant registration initiated for organization {OrganizationId}. Merchant ID: {MerchantId}",
                organizationId, merchantId);

            return Ok(new
            {
                success = true,
                merchantId = merchantId,
                message = "Merchant registration initiated successfully"
            });
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid merchant registration data");
            return BadRequest(new { success = false, message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Invalid operation during merchant registration");
            return BadRequest(new { success = false, message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during merchant registration");
            return StatusCode(500, new { success = false, message = "Internal server error" });
        }
    }

    /// <summary>
    /// Uploads a KYC document for the merchant
    /// </summary>
    [HttpPost("documents")]
    public async Task<IActionResult> UploadDocument([FromForm] UploadDocumentRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            if (request.File == null || request.File.Length == 0)
            {
                return BadRequest(new { success = false, message = "No file provided" });
            }

            var organizationId = GetOrganizationIdFromClaims();
            
            using var memoryStream = new MemoryStream();
            await request.File.CopyToAsync(memoryStream, cancellationToken);
            var fileContent = memoryStream.ToArray();

            await _merchantOnboardingService.UploadKycDocumentAsync(
                organizationId,
                $"{Guid.NewGuid()}_{request.File.FileName}",
                request.File.FileName,
                request.DocumentType,
                request.File.ContentType,
                fileContent,
                cancellationToken);

            _logger.LogInformation("KYC document {DocumentType} uploaded for organization {OrganizationId}",
                request.DocumentType, organizationId);

            return Ok(new
            {
                success = true,
                message = "Document uploaded successfully"
            });
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid document upload data");
            return BadRequest(new { success = false, message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Invalid operation during document upload");
            return BadRequest(new { success = false, message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during document upload");
            return StatusCode(500, new { success = false, message = "Internal server error" });
        }
    }

    /// <summary>
    /// Submits the merchant application for approval
    /// </summary>
    [HttpPost("submit")]
    public async Task<IActionResult> SubmitForApproval(CancellationToken cancellationToken = default)
    {
        try
        {
            var organizationId = GetOrganizationIdFromClaims();
            
            var success = await _merchantOnboardingService.SubmitForApprovalAsync(organizationId, cancellationToken);

            if (success)
            {
                _logger.LogInformation("Merchant application submitted for approval. Organization ID: {OrganizationId}",
                    organizationId);

                return Ok(new
                {
                    success = true,
                    message = "Application submitted for approval successfully"
                });
            }
            else
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Cannot submit application. Please ensure all required documents are uploaded."
                });
            }
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Invalid operation during application submission");
            return BadRequest(new { success = false, message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during application submission");
            return StatusCode(500, new { success = false, message = "Internal server error" });
        }
    }

    /// <summary>
    /// Gets the current status of the merchant application
    /// </summary>
    [HttpGet("status")]
    public async Task<IActionResult> GetMerchantStatus(CancellationToken cancellationToken = default)
    {
        try
        {
            var organizationId = GetOrganizationIdFromClaims();
            
            // This would typically get the organization and return its current status
            // For now, return a placeholder implementation
            return Ok(new
            {
                success = true,
                status = "pending",
                message = "Merchant application is being processed"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error retrieving merchant status");
            return StatusCode(500, new { success = false, message = "Internal server error" });
        }
    }

    private Guid GetOrganizationIdFromClaims()
    {
        var organizationIdClaim = User.FindFirst("organization_id")?.Value;
        if (string.IsNullOrEmpty(organizationIdClaim) || !Guid.TryParse(organizationIdClaim, out var organizationId))
        {
            throw new UnauthorizedAccessException("Organization ID not found in user claims");
        }
        return organizationId;
    }
}

public record RegisterMerchantRequest
{
    [Required]
    [MaxLength(200)]
    public string LegalBusinessName { get; init; } = string.Empty;

    [Required]
    [RegularExpression(@"^\d{10}$", ErrorMessage = "Tax ID must be exactly 10 digits")]
    public string TaxId { get; init; } = string.Empty;

    [MaxLength(20)]
    public string? KrsNumber { get; init; }

    [Required]
    [RegularExpression(@"^PL\d{26}$", ErrorMessage = "Bank account must be a valid Polish IBAN")]
    public string BankAccount { get; init; } = string.Empty;
}

public record UploadDocumentRequest
{
    [Required]
    public IFormFile File { get; init; } = null!;

    [Required]
    public DocumentType DocumentType { get; init; }
}