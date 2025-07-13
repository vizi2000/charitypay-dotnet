using CharityPay.Domain.Shared;

namespace CharityPay.Domain.Entities;

public sealed class OrganizationAnalytics : Entity
{
    private OrganizationAnalytics() { } // For EF Core

    private OrganizationAnalytics(Guid id, Guid organizationId, DateOnly date) : base(id)
    {
        OrganizationId = organizationId;
        Date = date;
        CreatedAt = DateTimeOffset.UtcNow;
    }

    public Guid OrganizationId { get; private set; }
    public DateOnly Date { get; private set; }
    public decimal TotalAmount { get; private set; }
    public int PaymentCount { get; private set; }
    public int UniqueVisitors { get; private set; }
    public int PageViews { get; private set; }
    public decimal ConversionRate { get; private set; }
    public decimal AverageDonation { get; private set; }
    public int RefundCount { get; private set; }
    public decimal RefundAmount { get; private set; }
    
    // Navigation properties
    public Organization Organization { get; private set; } = null!;

    public static OrganizationAnalytics Create(Guid organizationId, DateOnly date)
    {
        return new OrganizationAnalytics(Guid.NewGuid(), organizationId, date);
    }

    public void UpdatePaymentStats(decimal totalAmount, int paymentCount, decimal averageDonation)
    {
        TotalAmount = totalAmount;
        PaymentCount = paymentCount;
        AverageDonation = averageDonation;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void UpdateTrafficStats(int uniqueVisitors, int pageViews, decimal conversionRate)
    {
        UniqueVisitors = uniqueVisitors;
        PageViews = pageViews;
        ConversionRate = conversionRate;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void UpdateRefundStats(int refundCount, decimal refundAmount)
    {
        RefundCount = refundCount;
        RefundAmount = refundAmount;
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}