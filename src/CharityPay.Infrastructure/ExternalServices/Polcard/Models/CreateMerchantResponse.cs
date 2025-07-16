namespace CharityPay.Infrastructure.ExternalServices.Polcard.Models;

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

public record AuthTokenResponse
{
    public string AccessToken { get; init; } = string.Empty;
    public string TokenType { get; init; } = "Bearer";
    public int ExpiresIn { get; init; }
    public DateTime ExpiresAt { get; init; }
}