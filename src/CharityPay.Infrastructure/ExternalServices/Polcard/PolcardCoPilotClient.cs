using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using CharityPay.Application.Abstractions.Services;
using CharityPay.Domain.Entities;
using CharityPay.Infrastructure.ExternalServices.Polcard.Configuration;
using CharityPay.Infrastructure.ExternalServices.Polcard.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using CreateMerchantResponse = CharityPay.Application.Abstractions.Services.CreateMerchantResponse;
using DocumentUploadResponse = CharityPay.Application.Abstractions.Services.DocumentUploadResponse;
using MerchantStatusResponse = CharityPay.Application.Abstractions.Services.MerchantStatusResponse;
using WebhookEvent = CharityPay.Application.Abstractions.Services.WebhookEvent;

namespace CharityPay.Infrastructure.ExternalServices.Polcard;

public class PolcardCoPilotClient : IPolcardCoPilotClient
{
    private readonly HttpClient _httpClient;
    private readonly PolcardSettings _settings;
    private readonly ILogger<PolcardCoPilotClient> _logger;
    private AuthTokenResponse? _currentToken;
    private readonly SemaphoreSlim _tokenLock = new(1, 1);

    public PolcardCoPilotClient(
        HttpClient httpClient,
        IOptions<PolcardSettings> settings,
        ILogger<PolcardCoPilotClient> logger)
    {
        _httpClient = httpClient;
        _settings = settings.Value;
        _logger = logger;
        
        _httpClient.BaseAddress = new Uri(_settings.BaseUrl);
        _httpClient.Timeout = TimeSpan.FromSeconds(_settings.RequestTimeoutSeconds);
    }

    public async Task<CreateMerchantResponse> CreateMerchantAsync(Organization organization, CancellationToken cancellationToken = default)
    {
        await EnsureAuthenticatedAsync(cancellationToken);

        var request = MapOrganizationToMerchantRequest(organization);
        var json = JsonSerializer.Serialize(request, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });

        _logger.LogInformation("Creating merchant for organization {OrganizationId} with name {Name}", 
            organization.Id, organization.Name);

        try
        {
            var response = await _httpClient.PostAsync(_settings.MerchantEndpoint, 
                new StringContent(json, Encoding.UTF8, "application/json"), cancellationToken);

            var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                var merchantResponse = JsonSerializer.Deserialize<CreateMerchantResponse>(responseContent, 
                    new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });

                _logger.LogInformation("Successfully created merchant {MerchantId} for organization {OrganizationId}", 
                    merchantResponse?.MerchantId, organization.Id);

                return merchantResponse ?? throw new InvalidOperationException("Empty response from Polcard API");
            }

            _logger.LogError("Failed to create merchant for organization {OrganizationId}. Status: {StatusCode}, Response: {Response}", 
                organization.Id, response.StatusCode, responseContent);

            throw new HttpRequestException($"Polcard API error: {response.StatusCode} - {responseContent}");
        }
        catch (Exception ex) when (ex is not HttpRequestException)
        {
            _logger.LogError(ex, "Exception occurred while creating merchant for organization {OrganizationId}", organization.Id);
            throw;
        }
    }

    public async Task<DocumentUploadResponse> UploadDocumentAsync(string merchantId, Document document, byte[] fileContent, CancellationToken cancellationToken = default)
    {
        await EnsureAuthenticatedAsync(cancellationToken);

        var base64Content = Convert.ToBase64String(fileContent);
        var request = new DocumentUploadRequest
        {
            MerchantId = merchantId,
            DocumentCategoryCd = DocumentCategoryMapping.MapDocumentType(document.Type),
            FileName = document.OriginalFileName,
            MimeType = document.MimeType,
            Base64Content = base64Content
        };

        var json = JsonSerializer.Serialize(request, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });

        _logger.LogInformation("Uploading document {DocumentId} for merchant {MerchantId}", document.Id, merchantId);

        try
        {
            var response = await _httpClient.PostAsync($"{_settings.DocumentEndpoint}/{merchantId}/documents",
                new StringContent(json, Encoding.UTF8, "application/json"), cancellationToken);

            var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                var uploadResponse = JsonSerializer.Deserialize<DocumentUploadResponse>(responseContent,
                    new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });

                _logger.LogInformation("Successfully uploaded document {DocumentId} for merchant {MerchantId}", 
                    document.Id, merchantId);

                return uploadResponse ?? throw new InvalidOperationException("Empty response from Polcard API");
            }

            _logger.LogError("Failed to upload document {DocumentId} for merchant {MerchantId}. Status: {StatusCode}, Response: {Response}",
                document.Id, merchantId, response.StatusCode, responseContent);

            throw new HttpRequestException($"Polcard API error: {response.StatusCode} - {responseContent}");
        }
        catch (Exception ex) when (ex is not HttpRequestException)
        {
            _logger.LogError(ex, "Exception occurred while uploading document {DocumentId} for merchant {MerchantId}",
                document.Id, merchantId);
            throw;
        }
    }

    public async Task<MerchantStatusResponse> GetMerchantStatusAsync(string merchantId, CancellationToken cancellationToken = default)
    {
        await EnsureAuthenticatedAsync(cancellationToken);

        _logger.LogDebug("Getting status for merchant {MerchantId}", merchantId);

        try
        {
            var response = await _httpClient.GetAsync($"{_settings.MerchantEndpoint}/{merchantId}/status", cancellationToken);
            var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                var statusResponse = JsonSerializer.Deserialize<MerchantStatusResponse>(responseContent,
                    new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });

                return statusResponse ?? throw new InvalidOperationException("Empty response from Polcard API");
            }

            _logger.LogError("Failed to get status for merchant {MerchantId}. Status: {StatusCode}, Response: {Response}",
                merchantId, response.StatusCode, responseContent);

            throw new HttpRequestException($"Polcard API error: {response.StatusCode} - {responseContent}");
        }
        catch (Exception ex) when (ex is not HttpRequestException)
        {
            _logger.LogError(ex, "Exception occurred while getting status for merchant {MerchantId}", merchantId);
            throw;
        }
    }

    public bool VerifyWebhookSignature(string payload, string signature, string secret)
    {
        try
        {
            var expectedSignature = CreateHmacSha256Signature(payload, secret);
            return string.Equals(signature, expectedSignature, StringComparison.OrdinalIgnoreCase);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error verifying webhook signature");
            return false;
        }
    }

    public WebhookEvent ParseWebhookEvent(string payload)
    {
        try
        {
            var webhookEvent = JsonSerializer.Deserialize<WebhookEvent>(payload,
                new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });

            return webhookEvent ?? throw new InvalidOperationException("Failed to parse webhook payload");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error parsing webhook event: {Payload}", payload);
            throw;
        }
    }

    private async Task EnsureAuthenticatedAsync(CancellationToken cancellationToken)
    {
        if (_currentToken != null && _currentToken.ExpiresAt > DateTime.UtcNow.AddMinutes(_settings.TokenExpirationBufferMinutes))
        {
            _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue(
                _currentToken.TokenType, _currentToken.AccessToken);
            return;
        }

        await _tokenLock.WaitAsync(cancellationToken);
        try
        {
            // Double-check after acquiring lock
            if (_currentToken != null && _currentToken.ExpiresAt > DateTime.UtcNow.AddMinutes(_settings.TokenExpirationBufferMinutes))
            {
                _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue(
                    _currentToken.TokenType, _currentToken.AccessToken);
                return;
            }

            await RefreshTokenAsync(cancellationToken);
        }
        finally
        {
            _tokenLock.Release();
        }
    }

    private async Task RefreshTokenAsync(CancellationToken cancellationToken)
    {
        _logger.LogDebug("Refreshing Polcard authentication token");

        var request = new FormUrlEncodedContent(new[]
        {
            new KeyValuePair<string, string>("grant_type", "client_credentials"),
            new KeyValuePair<string, string>("client_id", _settings.ClientId),
            new KeyValuePair<string, string>("client_secret", _settings.ClientSecret)
        });

        try
        {
            var response = await _httpClient.PostAsync(_settings.TokenEndpoint, request, cancellationToken);
            var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                var tokenResponse = JsonSerializer.Deserialize<AuthTokenResponse>(responseContent,
                    new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });

                if (tokenResponse != null)
                {
                    _currentToken = tokenResponse with 
                    { 
                        ExpiresAt = DateTime.UtcNow.AddSeconds(tokenResponse.ExpiresIn) 
                    };

                    _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue(
                        _currentToken.TokenType, _currentToken.AccessToken);

                    _logger.LogDebug("Successfully refreshed Polcard authentication token");
                }
                else
                {
                    throw new InvalidOperationException("Empty token response from Polcard");
                }
            }
            else
            {
                _logger.LogError("Failed to refresh token. Status: {StatusCode}, Response: {Response}",
                    response.StatusCode, responseContent);
                throw new HttpRequestException($"Authentication failed: {response.StatusCode} - {responseContent}");
            }
        }
        catch (Exception ex) when (ex is not HttpRequestException)
        {
            _logger.LogError(ex, "Exception occurred while refreshing authentication token");
            throw;
        }
    }

    private CreateMerchantRequest MapOrganizationToMerchantRequest(Organization organization)
    {
        if (organization.TaxId == null || organization.BankAccount == null)
            throw new InvalidOperationException("Organization must have TaxId and BankAccount for merchant creation");

        return new CreateMerchantRequest
        {
            TemplateId = _settings.DefaultTemplateId,
            Merchant = new MerchantDetails
            {
                LegalBusinessName = organization.LegalBusinessName ?? organization.Name,
                DoingBusinessAs = organization.Name,
                TaxId = organization.TaxId.Value,
                BusinessIdTypeCd = "PTIN",
                MerchantCategoryCd = "8398", // Charitable organizations
                WebsiteUrl = organization.Website ?? string.Empty,
                ContactEmail = organization.ContactEmail,
                ContactPhone = organization.Phone ?? string.Empty
            },
            Addresses = new[]
            {
                new Address
                {
                    AddressTypeCd = "LEGAL",
                    Address1 = organization.Address ?? organization.Location,
                    City = ExtractCityFromLocation(organization.Location),
                    StateProvinceCd = ExtractStateFromLocation(organization.Location),
                    PostalCd = ExtractPostalCodeFromLocation(organization.Location),
                    CountryCd = "PL"
                }
            },
            Deposits = new[]
            {
                new DepositAccount
                {
                    DepositoryNumber = organization.BankAccount.Iban,
                    DepositoryTypeCd = "DDA",
                    CurrencyCd = "PLN"
                }
            },
            Persons = new[]
            {
                new PersonDetails
                {
                    FirstName = ExtractFirstName(organization.User?.FullName ?? ""),
                    LastName = ExtractLastName(organization.User?.FullName ?? ""),
                    PersonTypeCd = "OWNER",
                    Email = organization.ContactEmail,
                    Phone = organization.Phone ?? string.Empty
                }
            }
        };
    }

    private static string CreateHmacSha256Signature(string payload, string secret)
    {
        var keyBytes = Encoding.UTF8.GetBytes(secret);
        var payloadBytes = Encoding.UTF8.GetBytes(payload);

        using var hmac = new HMACSHA256(keyBytes);
        var hashBytes = hmac.ComputeHash(payloadBytes);
        return Convert.ToHexString(hashBytes).ToLowerInvariant();
    }

    // Helper methods for parsing location data
    private static string ExtractCityFromLocation(string location) => 
        location.Split(',').LastOrDefault()?.Trim() ?? string.Empty;

    private static string ExtractStateFromLocation(string location) => 
        location.Contains("woj.") ? location.Split("woj.").LastOrDefault()?.Trim() ?? string.Empty : string.Empty;

    private static string ExtractPostalCodeFromLocation(string location) => 
        System.Text.RegularExpressions.Regex.Match(location, @"\d{2}-\d{3}").Value;

    private static string ExtractFirstName(string fullName) => 
        fullName.Split(' ').FirstOrDefault() ?? string.Empty;

    private static string ExtractLastName(string fullName) => 
        string.Join(" ", fullName.Split(' ').Skip(1));
}