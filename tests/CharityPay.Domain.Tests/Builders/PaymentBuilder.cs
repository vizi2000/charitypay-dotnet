using CharityPay.Domain.Entities;
using CharityPay.Domain.Enums;

namespace CharityPay.Domain.Tests.Builders;

/// <summary>
/// Builder for <see cref="Payment"/> respecting domain invariants.
/// </summary>
public sealed class PaymentBuilder
{
    private decimal _amount = 100m;
    private PaymentMethod _method = PaymentMethod.Card;
    private string _donorName = "Test Donor";
    private string? _donorEmail = $"donor{Guid.NewGuid():N}@example.com";
    private string? _donorPhone = "+48123456789";
    private Guid _organizationId = Guid.NewGuid();

    private bool _complete;
    private bool _fail;
    private bool _cancel;
    private string? _transactionId;
    private string? _approvalCode;
    private string? _orderId;
    private string? _checkoutId;
    private string? _redirectUrl;

    public PaymentBuilder WithAmount(decimal amount)
    {
        _amount = amount;
        return this;
    }

    public PaymentBuilder WithMethod(PaymentMethod method)
    {
        _method = method;
        return this;
    }

    public PaymentBuilder WithOrganization(Organization organization)
    {
        _organizationId = organization.Id;
        return this;
    }

    public PaymentBuilder WithDonor(string name, string? email = null, string? phone = null)
    {
        _donorName = name;
        _donorEmail = email;
        _donorPhone = phone;
        return this;
    }

    public PaymentBuilder AsAnonymous()
    {
        _donorName = "Anonymous";
        return this;
    }

    public PaymentBuilder WithFiservDetails(string? orderId, string? checkoutId, string? redirectUrl)
    {
        _orderId = orderId;
        _checkoutId = checkoutId;
        _redirectUrl = redirectUrl;
        return this;
    }

    public PaymentBuilder Completed(string? transactionId = null, string? approvalCode = null)
    {
        _complete = true;
        _transactionId = transactionId;
        _approvalCode = approvalCode;
        return this;
    }

    public PaymentBuilder Failed()
    {
        _fail = true;
        return this;
    }

    public PaymentBuilder Cancelled()
    {
        _cancel = true;
        return this;
    }

    public Payment Build()
    {
        var payment = Payment.Create(
            _amount,
            _method,
            _organizationId,
            _donorName,
            _donorEmail,
            _donorPhone);

        if (_orderId != null || _checkoutId != null || _redirectUrl != null)
        {
            payment.UpdateFiservDetails(_orderId, _checkoutId, _redirectUrl);
        }

        if (_complete)
        {
            payment.Complete(_transactionId, _approvalCode);
        }
        else if (_fail)
        {
            payment.Fail();
        }
        else if (_cancel)
        {
            payment.Cancel();
        }

        return payment;
    }

    public List<Payment> BuildMany(int count)
    {
        var list = new List<Payment>();
        for (int i = 0; i < count; i++)
        {
            list.Add(new PaymentBuilder()
                .WithAmount(_amount)
                .WithMethod(_method)
                .WithDonor(_donorName, _donorEmail, _donorPhone)
                .WithFiservDetails(_orderId, _checkoutId, _redirectUrl)
                .Build());
        }
        return list;
    }
}
