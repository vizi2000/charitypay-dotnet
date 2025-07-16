using Microsoft.AspNetCore.Mvc;
using CharityPay.Application.Services;
using CharityPay.Application.Abstractions.Services;
using CharityPay.Infrastructure.ExternalServices.Polcard.Configuration;
using Microsoft.Extensions.Options;

namespace CharityPay.API.Controllers;

[ApiController]
[Route("api/v1/webhooks/polcard")]
public class PolcardWebhookController : ControllerBase
{
    private readonly IMerchantOnboardingService _merchantOnboardingService;
    private readonly IPolcardCoPilotClient _polcardClient;
    private readonly PolcardSettings _polcardSettings;
    private readonly ILogger<PolcardWebhookController> _logger;

    public PolcardWebhookController(
        IMerchantOnboardingService merchantOnboardingService,
        IPolcardCoPilotClient polcardClient,
        IOptions<PolcardSettings> polcardSettings,
        ILogger<PolcardWebhookController> logger)
    {
        _merchantOnboardingService = merchantOnboardingService;
        _polcardClient = polcardClient;
        _polcardSettings = polcardSettings.Value;
        _logger = logger;
    }

    /// <summary>
    /// Receives webhook notifications from Polcard about merchant status changes
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> HandleWebhook([FromBody] object payload, CancellationToken cancellationToken = default)
    {
        try
        {
            // Get raw payload as string
            var rawPayload = payload.ToString() ?? string.Empty;
            
            // Get signature from headers
            if (!Request.Headers.TryGetValue("X-Signature", out var signatureHeader))
            {
                _logger.LogWarning("Webhook received without signature header");
                return BadRequest("Missing signature header");
            }

            var signature = signatureHeader.ToString();

            // Verify webhook signature
            if (!_polcardClient.VerifyWebhookSignature(rawPayload, signature, _polcardSettings.WebhookSecret))
            {
                _logger.LogWarning("Webhook signature verification failed. Signature: {Signature}", signature);
                return Unauthorized("Invalid signature");
            }

            // Parse webhook event
            var webhookEvent = _polcardClient.ParseWebhookEvent(rawPayload);

            _logger.LogInformation("Processing webhook event {EventType} for merchant {MerchantId} with status {Status}",
                webhookEvent.EventType, webhookEvent.MerchantId, webhookEvent.Status);

            // Process the status update
            await _merchantOnboardingService.ProcessMerchantStatusUpdateAsync(
                webhookEvent.MerchantId, 
                webhookEvent.Status, 
                webhookEvent.Reason, 
                cancellationToken);

            _logger.LogInformation("Successfully processed webhook for merchant {MerchantId}", webhookEvent.MerchantId);

            return Ok(new { message = "Webhook processed successfully" });
        }
        catch (ArgumentException ex)
        {
            _logger.LogError(ex, "Invalid webhook payload format");
            return BadRequest($"Invalid payload format: {ex.Message}");
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "Invalid operation while processing webhook");
            return BadRequest($"Invalid operation: {ex.Message}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error processing webhook");
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Health check endpoint for webhook service
    /// </summary>
    [HttpGet("health")]
    public IActionResult Health()
    {
        return Ok(new 
        { 
            status = "healthy", 
            timestamp = DateTimeOffset.UtcNow,
            service = "polcard-webhook" 
        });
    }
}