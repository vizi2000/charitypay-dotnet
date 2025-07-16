using CharityPay.Domain.Enums;
using CharityPay.Domain.Shared;
using CharityPay.Domain.ValueObjects;

namespace CharityPay.Domain.Entities;

public sealed class Organization : Entity
{
    private readonly List<Payment> _payments = new();
    private readonly List<Document> _documents = new();
    
    private Organization() { } // For EF Core

    private Organization(Guid id, string name, string description, string category, string location,
        decimal targetAmount, string contactEmail, Guid userId) : base(id)
    {
        Name = name;
        Description = description;
        Category = category;
        Location = location;
        TargetAmount = targetAmount;
        ContactEmail = contactEmail;
        UserId = userId;
        CollectedAmount = 0;
        Status = OrganizationStatus.Pending;
        CreatedAt = DateTimeOffset.UtcNow;
    }

    public string Name { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public string Category { get; private set; } = string.Empty;
    public string Location { get; private set; } = string.Empty;
    public decimal TargetAmount { get; private set; }
    public decimal CollectedAmount { get; private set; }
    public string ContactEmail { get; private set; } = string.Empty;
    public string? Website { get; private set; }
    public string? Phone { get; private set; }
    public string? Address { get; private set; }
    public string? LogoUrl { get; private set; }
    public string? PrimaryColor { get; private set; }
    public string? SecondaryColor { get; private set; }
    public string? CustomMessage { get; private set; }
    public OrganizationStatus Status { get; private set; }
    public string? AdminNotes { get; private set; }
    public Guid UserId { get; private set; }
    
    // Merchant-specific fields
    public Nip? TaxId { get; private set; }
    public string? LegalBusinessName { get; private set; }
    public string? KrsNumber { get; private set; }
    public BankAccount? BankAccount { get; private set; }
    public string? PolcardMerchantId { get; private set; }
    
    // Navigation properties
    public User User { get; private set; } = null!;
    public IReadOnlyCollection<Payment> Payments => _payments.AsReadOnly();
    public IReadOnlyCollection<Document> Documents => _documents.AsReadOnly();

    public static Organization Create(string name, string description, string category, string location,
        decimal targetAmount, string contactEmail, Guid userId)
    {
        return new Organization(Guid.NewGuid(), name, description, category, location, targetAmount, contactEmail, userId);
    }

    public void UpdateProfile(string? website, string? phone, string? address, string? primaryColor,
        string? secondaryColor, string? customMessage)
    {
        Website = website;
        Phone = phone;
        Address = address;
        PrimaryColor = primaryColor;
        SecondaryColor = secondaryColor;
        CustomMessage = customMessage;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void UpdateLogo(string logoUrl)
    {
        LogoUrl = logoUrl;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void Approve(string? adminNotes = null)
    {
        Status = OrganizationStatus.Active;
        AdminNotes = adminNotes;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void Reject(string? adminNotes = null)
    {
        Status = OrganizationStatus.Rejected;
        AdminNotes = adminNotes;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void UpdateMerchantDetails(string legalBusinessName, Nip taxId, string? krsNumber, BankAccount bankAccount)
    {
        LegalBusinessName = legalBusinessName;
        TaxId = taxId;
        KrsNumber = krsNumber;
        BankAccount = bankAccount;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void SubmitKyc()
    {
        if (Status != OrganizationStatus.Pending)
            throw new InvalidOperationException("Can only submit KYC from Pending status");
            
        Status = OrganizationStatus.KycSubmitted;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void ApproveMerchant(string polcardMerchantId, string? adminNotes = null)
    {
        if (Status != OrganizationStatus.KycSubmitted)
            throw new InvalidOperationException("Can only approve merchant from KycSubmitted status");
            
        Status = OrganizationStatus.MerchantApproved;
        PolcardMerchantId = polcardMerchantId;
        AdminNotes = adminNotes;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void ActivateMerchant(string? adminNotes = null)
    {
        if (Status != OrganizationStatus.MerchantApproved)
            throw new InvalidOperationException("Can only activate from MerchantApproved status");
            
        Status = OrganizationStatus.Active;
        AdminNotes = adminNotes;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void Suspend(string? adminNotes = null)
    {
        Status = OrganizationStatus.Suspended;
        AdminNotes = adminNotes;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void AddPayment(Payment payment)
    {
        _payments.Add(payment);
        if (payment.Status == PaymentStatus.Completed)
        {
            CollectedAmount += payment.Amount;
        }
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void UpdateCollectedAmount(decimal amount)
    {
        CollectedAmount += amount;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void AddDocument(Document document)
    {
        _documents.Add(document);
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}