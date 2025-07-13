using CharityPay.Domain.Shared;

namespace CharityPay.Domain.Entities;

public sealed class PaymentEvent : Entity
{
    private PaymentEvent() { } // For EF Core

    private PaymentEvent(Guid id, Guid paymentId, Guid organizationId, string eventType, decimal amount,
        string status, string method) : base(id)
    {
        PaymentId = paymentId;
        OrganizationId = organizationId;
        EventType = eventType;
        Amount = amount;
        Status = status;
        Method = method;
        Timestamp = DateTimeOffset.UtcNow;
    }

    public Guid PaymentId { get; private set; }
    public Guid OrganizationId { get; private set; }
    public string EventType { get; private set; } = string.Empty;
    public decimal Amount { get; private set; }
    public string Status { get; private set; } = string.Empty;
    public string Method { get; private set; } = string.Empty;
    public string? DonorEmail { get; private set; }
    public string? DonorName { get; private set; }
    public string? IpAddress { get; private set; }
    public string? UserAgent { get; private set; }
    public string? DeviceType { get; private set; }
    public string? Country { get; private set; }
    public DateTimeOffset Timestamp { get; private set; }
    public string? Metadata { get; private set; }
    
    // Navigation properties
    public Payment Payment { get; private set; } = null!;
    public Organization Organization { get; private set; } = null!;

    public static PaymentEvent Create(Guid paymentId, Guid organizationId, string eventType, decimal amount,
        string status, string method)
    {
        return new PaymentEvent(Guid.NewGuid(), paymentId, organizationId, eventType, amount, status, method);
    }

    public void AddDonorInfo(string? donorEmail, string? donorName)
    {
        DonorEmail = donorEmail;
        DonorName = donorName;
    }

    public void AddRequestInfo(string? ipAddress, string? userAgent, string? deviceType, string? country)
    {
        IpAddress = ipAddress;
        UserAgent = userAgent;
        DeviceType = deviceType;
        Country = country;
    }

    public void AddMetadata(string? metadata)
    {
        Metadata = metadata;
    }
}