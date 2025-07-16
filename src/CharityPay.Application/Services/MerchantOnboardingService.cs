using AutoMapper;
using Microsoft.Extensions.Logging;
using CharityPay.Application.Abstractions;
using CharityPay.Application.Abstractions.Services;
using CharityPay.Domain.Entities;
using CharityPay.Domain.Enums;
using CharityPay.Domain.ValueObjects;

namespace CharityPay.Application.Services;

public interface IMerchantOnboardingService
{
    Task<string> InitiateMerchantRegistrationAsync(Guid organizationId, string legalBusinessName, 
        string taxId, string? krsNumber, string bankAccount, CancellationToken cancellationToken = default);
    
    Task UploadKycDocumentAsync(Guid organizationId, string fileName, string originalFileName, 
        DocumentType documentType, string mimeType, byte[] fileContent, CancellationToken cancellationToken = default);
    
    Task<bool> SubmitForApprovalAsync(Guid organizationId, CancellationToken cancellationToken = default);
    
    Task ProcessMerchantStatusUpdateAsync(string merchantId, string status, string? reason = null, 
        CancellationToken cancellationToken = default);
}

public class MerchantOnboardingService : IMerchantOnboardingService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPolcardCoPilotClient _polcardClient;
    private readonly ILogger<MerchantOnboardingService> _logger;

    public MerchantOnboardingService(
        IUnitOfWork unitOfWork,
        IPolcardCoPilotClient polcardClient,
        ILogger<MerchantOnboardingService> logger)
    {
        _unitOfWork = unitOfWork;
        _polcardClient = polcardClient;
        _logger = logger;
    }

    public async Task<string> InitiateMerchantRegistrationAsync(Guid organizationId, string legalBusinessName,
        string taxId, string? krsNumber, string bankAccount, CancellationToken cancellationToken = default)
    {
        var organization = await _unitOfWork.Organizations.GetByIdAsync(organizationId, cancellationToken);
        if (organization == null)
        {
            throw new InvalidOperationException($"Organization {organizationId} not found");
        }

        if (organization.Status != OrganizationStatus.Pending)
        {
            throw new InvalidOperationException($"Organization {organizationId} is not in Pending status");
        }

        try
        {
            // Validate and set merchant details
            var nip = new Nip(taxId);
            var iban = new BankAccount(bankAccount);
            
            organization.UpdateMerchantDetails(legalBusinessName, nip, krsNumber, iban);

            // Create merchant in Polcard
            var merchantResponse = await _polcardClient.CreateMerchantAsync(organization, cancellationToken);
            
            // Update organization with Polcard merchant ID
            organization.ApproveMerchant(merchantResponse.MerchantId, "Merchant created in Polcard");

            _unitOfWork.Organizations.Update(organization);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Successfully initiated merchant registration for organization {OrganizationId}. " +
                                 "Polcard Merchant ID: {MerchantId}", organizationId, merchantResponse.MerchantId);

            return merchantResponse.MerchantId;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to initiate merchant registration for organization {OrganizationId}", organizationId);
            
            // Mark organization as rejected if Polcard creation fails
            organization.Reject($"Failed to create merchant: {ex.Message}");
            _unitOfWork.Organizations.Update(organization);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            
            throw;
        }
    }

    public async Task UploadKycDocumentAsync(Guid organizationId, string fileName, string originalFileName,
        DocumentType documentType, string mimeType, byte[] fileContent, CancellationToken cancellationToken = default)
    {
        var organization = await _unitOfWork.Organizations.GetByIdAsync(organizationId, cancellationToken);
        if (organization == null)
        {
            throw new InvalidOperationException($"Organization {organizationId} not found");
        }

        if (string.IsNullOrEmpty(organization.PolcardMerchantId))
        {
            throw new InvalidOperationException($"Organization {organizationId} does not have a Polcard merchant ID");
        }

        try
        {
            // Create document entity
            var document = Document.Create(fileName, originalFileName, documentType, mimeType, 
                fileContent.Length, $"documents/{organizationId}/{fileName}", organizationId);

            // Upload to Polcard
            var uploadResponse = await _polcardClient.UploadDocumentAsync(
                organization.PolcardMerchantId, document, fileContent, cancellationToken);

            // Mark document as verified if upload was successful
            if (uploadResponse.Status == "SUCCESS")
            {
                document.MarkAsVerified($"Uploaded to Polcard: {uploadResponse.DocumentId}");
            }

            // Add document to organization
            organization.AddDocument(document);
            _unitOfWork.Organizations.Update(organization);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Successfully uploaded KYC document {DocumentType} for organization {OrganizationId}",
                documentType, organizationId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to upload KYC document {DocumentType} for organization {OrganizationId}",
                documentType, organizationId);
            throw;
        }
    }

    public async Task<bool> SubmitForApprovalAsync(Guid organizationId, CancellationToken cancellationToken = default)
    {
        var organization = await _unitOfWork.Organizations.GetByIdAsync(organizationId, cancellationToken);
        if (organization == null)
        {
            throw new InvalidOperationException($"Organization {organizationId} not found");
        }

        if (organization.Status != OrganizationStatus.Pending && organization.Status != OrganizationStatus.MerchantApproved)
        {
            throw new InvalidOperationException($"Organization {organizationId} cannot be submitted for approval from current status");
        }

        // Validate that required documents are uploaded
        var requiredDocuments = new[] { DocumentType.CorporateDocument, DocumentType.GovernmentId, DocumentType.BankStatement };
        var uploadedDocumentTypes = organization.Documents.Select(d => d.Type).ToHashSet();

        var missingDocuments = requiredDocuments.Where(d => !uploadedDocumentTypes.Contains(d)).ToList();
        if (missingDocuments.Any())
        {
            _logger.LogWarning("Cannot submit organization {OrganizationId} for approval. Missing documents: {MissingDocuments}",
                organizationId, string.Join(", ", missingDocuments));
            return false;
        }

        try
        {
            // Submit KYC to Polcard (this might be automatic after document upload)
            organization.SubmitKyc();
            _unitOfWork.Organizations.Update(organization);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Successfully submitted organization {OrganizationId} for KYC approval", organizationId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to submit organization {OrganizationId} for approval", organizationId);
            throw;
        }
    }

    public async Task ProcessMerchantStatusUpdateAsync(string merchantId, string status, string? reason = null,
        CancellationToken cancellationToken = default)
    {
        // Find organization by Polcard merchant ID
        var organization = await _unitOfWork.Organizations.GetByPolcardMerchantIdAsync(merchantId, cancellationToken);
        if (organization == null)
        {
            _logger.LogWarning("Received status update for unknown merchant ID: {MerchantId}", merchantId);
            return;
        }

        _logger.LogInformation("Processing status update for merchant {MerchantId}, organization {OrganizationId}. " +
                             "Status: {Status}, Reason: {Reason}", merchantId, organization.Id, status, reason);

        try
        {
            switch (status.ToUpperInvariant())
            {
                case "APPROVED":
                    if (organization.Status == OrganizationStatus.KycSubmitted)
                    {
                        organization.ActivateMerchant($"Approved by Polcard. {reason}");
                        _logger.LogInformation("Merchant {MerchantId} approved and activated", merchantId);
                    }
                    break;

                case "REJECTED":
                    organization.Reject($"Rejected by Polcard. {reason}");
                    _logger.LogInformation("Merchant {MerchantId} rejected", merchantId);
                    break;

                case "PENDING":
                case "UNDER_REVIEW":
                    if (organization.Status == OrganizationStatus.MerchantApproved)
                    {
                        organization.SubmitKyc();
                    }
                    _logger.LogInformation("Merchant {MerchantId} under review", merchantId);
                    break;

                case "SUSPENDED":
                    organization.Suspend($"Suspended by Polcard. {reason}");
                    _logger.LogInformation("Merchant {MerchantId} suspended", merchantId);
                    break;

                default:
                    _logger.LogWarning("Unknown status '{Status}' for merchant {MerchantId}", status, merchantId);
                    return;
            }

            _unitOfWork.Organizations.Update(organization);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Successfully processed status update for merchant {MerchantId}", merchantId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to process status update for merchant {MerchantId}", merchantId);
            throw;
        }
    }
}