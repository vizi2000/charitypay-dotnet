using CharityPay.Domain.Enums;

namespace CharityPay.Infrastructure.ExternalServices.Polcard.Models;

public record DocumentUploadRequest
{
    public string MerchantId { get; init; } = string.Empty;
    public string DocumentCategoryCd { get; init; } = string.Empty;
    public string FileName { get; init; } = string.Empty;
    public string MimeType { get; init; } = string.Empty;
    public string Base64Content { get; init; } = string.Empty;
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

public static class DocumentCategoryMapping
{
    public static string MapDocumentType(DocumentType documentType) => documentType switch
    {
        DocumentType.CorporateDocument => "CORP_DOC",
        DocumentType.GovernmentId => "GOV_ID",
        DocumentType.BankStatement => "BANK_STMT",
        DocumentType.TaxCertificate => "TAX_CERT",
        DocumentType.AuthorizationLetter => "AUTH_LETTER",
        _ => "OTHER"
    };
}