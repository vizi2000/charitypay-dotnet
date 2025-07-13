using CharityPay.Domain.Enums;
using CharityPay.Domain.Shared;

namespace CharityPay.Domain.Entities;

public sealed class Organization : Entity
{
    private readonly List<Payment> _payments = new();
    
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
    
    // Navigation properties
    public User User { get; private set; } = null!;
    public IReadOnlyCollection<Payment> Payments => _payments.AsReadOnly();

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
        Status = OrganizationStatus.Approved;
        AdminNotes = adminNotes;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void Reject(string? adminNotes = null)
    {
        Status = OrganizationStatus.Rejected;
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
}