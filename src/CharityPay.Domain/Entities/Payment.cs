using CharityPay.Domain.Enums;
using CharityPay.Domain.Shared;

namespace CharityPay.Domain.Entities;

public sealed class Payment : Entity
{
    private Payment() { } // For EF Core

    private Payment(Guid id, decimal amount, PaymentMethod method, string donorName, string? donorEmail, 
        string? donorPhone, Guid organizationId) : base(id)
    {
        Amount = amount;
        Method = method;
        DonorName = donorName;
        DonorEmail = donorEmail;
        DonorPhone = donorPhone;
        OrganizationId = organizationId;
        Status = PaymentStatus.Pending;
        CreatedAt = DateTimeOffset.UtcNow;
    }

    public decimal Amount { get; private set; }
    public PaymentStatus Status { get; private set; }
    public PaymentMethod Method { get; private set; }
    public PaymentMethod PaymentMethod => Method; // Alias for compatibility
    public string DonorName { get; private set; } = "Anonymous";
    public string? DonorEmail { get; private set; }
    public string? DonorPhone { get; private set; }
    public Guid OrganizationId { get; private set; }
    public string? FiservOrderId { get; private set; }
    public string? FiservCheckoutId { get; private set; }
    public string? FiservTransactionId { get; private set; }
    public string? ApprovalCode { get; private set; }
    public string? RedirectUrl { get; private set; }
    
    // Navigation properties
    public Organization Organization { get; private set; } = null!;

    public static Payment Create(decimal amount, PaymentMethod method, Guid organizationId,
        string donorName, string? donorEmail = null, string? donorPhone = null)
    {
        return new Payment(Guid.NewGuid(), amount, method, donorName, donorEmail, donorPhone, organizationId);
    }

    public void UpdateFiservDetails(string? orderId, string? checkoutId, string? redirectUrl)
    {
        FiservOrderId = orderId;
        FiservCheckoutId = checkoutId;
        RedirectUrl = redirectUrl;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void Complete(string? transactionId, string? approvalCode)
    {
        Status = PaymentStatus.Completed;
        FiservTransactionId = transactionId;
        ApprovalCode = approvalCode;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void Fail()
    {
        Status = PaymentStatus.Failed;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void Cancel()
    {
        Status = PaymentStatus.Cancelled;
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}