using CharityPay.Domain.Entities;

namespace CharityPay.Application.Abstractions.Services;

public interface IPolcardCoPilotClient
{
    /// <summary>
    /// Creates a new merchant in Polcard CoPilot system
    /// </summary>
    Task<CreateMerchantResponse> CreateMerchantAsync(Organization organization, CancellationToken cancellationToken = default);

    /// <summary>
    /// Uploads a KYC document for the merchant
    /// </summary>
    Task<DocumentUploadResponse> UploadDocumentAsync(string merchantId, Document document, byte[] fileContent, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the current status of a merchant application
    /// </summary>
    Task<MerchantStatusResponse> GetMerchantStatusAsync(string merchantId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Verifies webhook signature for security
    /// </summary>
    bool VerifyWebhookSignature(string payload, string signature, string secret);

    /// <summary>
    /// Parses webhook payload into structured event
    /// </summary>
    WebhookEvent ParseWebhookEvent(string payload);
}

public record CreateMerchantResponse
{
    public string MerchantId { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public string Message { get; init; } = string.Empty;
    public DateTime CreatedAt { get; init; }
}

public record MerchantStatusResponse
{
    public string MerchantId { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public string Reason { get; init; } = string.Empty;
    public DateTime UpdatedAt { get; init; }
}

public record DocumentUploadResponse
{
    public string DocumentId { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public string Message { get; init; } = string.Empty;
}

public record WebhookEvent
{
    public string EventType { get; init; } = string.Empty;
    public string MerchantId { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public string Reason { get; init; } = string.Empty;
    public DateTime Timestamp { get; init; }
    public Dictionary<string, object> AdditionalData { get; init; } = new();
}