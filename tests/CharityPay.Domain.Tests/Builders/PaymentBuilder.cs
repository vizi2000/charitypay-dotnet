using CharityPay.Domain.Entities;
using CharityPay.Domain.Enums;

namespace CharityPay.Domain.Tests.Builders;

public class PaymentBuilder
{
    private Payment _payment;

    public PaymentBuilder()
    {
        _payment = new Payment
        {
            Id = Guid.NewGuid(),
            Amount = 100.00m,
            Currency = "PLN",
            Status = PaymentStatus.Pending,
            Method = PaymentMethod.Card,
            DonorEmail = $"donor{Guid.NewGuid():N}@example.com",
            DonorName = "Test Donor",
            DonorPhone = "+48123456789",
            IsAnonymous = false,
            Message = "Test donation message",
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow,
            TransactionId = $"TEST_{Guid.NewGuid():N}",
            ProviderOrderId = $"ORDER_{Guid.NewGuid():N}"
        };
    }

    public PaymentBuilder WithId(Guid id)
    {
        _payment.Id = id;
        return this;
    }

    public PaymentBuilder WithAmount(decimal amount)
    {
        _payment.Amount = amount;
        return this;
    }

    public PaymentBuilder WithOrganization(Organization organization)
    {
        _payment.OrganizationId = organization.Id;
        _payment.Organization = organization;
        return this;
    }

    public PaymentBuilder WithDonor(string email, string name, string phone = null)
    {
        _payment.DonorEmail = email;
        _payment.DonorName = name;
        _payment.DonorPhone = phone;
        return this;
    }

    public PaymentBuilder AsAnonymous()
    {
        _payment.IsAnonymous = true;
        _payment.DonorName = "Anonymous";
        return this;
    }

    public PaymentBuilder WithMethod(PaymentMethod method)
    {
        _payment.Method = method;
        return this;
    }

    public PaymentBuilder AsPending()
    {
        _payment.Status = PaymentStatus.Pending;
        _payment.CompletedAt = null;
        return this;
    }

    public PaymentBuilder AsCompleted()
    {
        _payment.Status = PaymentStatus.Completed;
        _payment.CompletedAt = DateTimeOffset.UtcNow;
        return this;
    }

    public PaymentBuilder AsFailed()
    {
        _payment.Status = PaymentStatus.Failed;
        _payment.FailureReason = "Test failure reason";
        return this;
    }

    public PaymentBuilder AsCancelled()
    {
        _payment.Status = PaymentStatus.Cancelled;
        return this;
    }

    public PaymentBuilder WithTransactionDetails(string transactionId, string providerOrderId)
    {
        _payment.TransactionId = transactionId;
        _payment.ProviderOrderId = providerOrderId;
        return this;
    }

    public PaymentBuilder WithCreatedAt(DateTimeOffset createdAt)
    {
        _payment.CreatedAt = createdAt;
        _payment.UpdatedAt = createdAt;
        return this;
    }

    public Payment Build() => _payment;
    
    public List<Payment> BuildMany(int count)
    {
        var payments = new List<Payment>();
        for (int i = 0; i < count; i++)
        {
            payments.Add(new PaymentBuilder().Build());
        }
        return payments;
    }
}