namespace CharityPay.Infrastructure.ExternalServices.Polcard.Configuration;

public class PolcardSettings
{
    public const string SectionName = "PolcardSettings";

    public string BaseUrl { get; set; } = "https://api.copilot.fiserv.com";
    public string ClientId { get; set; } = string.Empty;
    public string ClientSecret { get; set; } = string.Empty;
    public string WebhookSecret { get; set; } = string.Empty;
    public string DefaultTemplateId { get; set; } = "992"; // Charity template
    public int TokenExpirationBufferMinutes { get; set; } = 5;
    public int RequestTimeoutSeconds { get; set; } = 30;
    public bool UseSandbox { get; set; } = true;
    
    public string TokenEndpoint => $"{BaseUrl}/oauth/token";
    public string MerchantEndpoint => $"{BaseUrl}/v1/merchants";
    public string DocumentEndpoint => $"{BaseUrl}/v1/documents";
}