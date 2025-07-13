using CharityPay.Domain.Shared;

namespace CharityPay.Domain.Entities;

public sealed class DeviceTransaction : Entity
{
    private DeviceTransaction() { } // For EF Core

    private DeviceTransaction(Guid id, Guid deviceId, string transactionType, string status) : base(id)
    {
        DeviceId = deviceId;
        TransactionType = transactionType;
        Status = status;
        Timestamp = DateTimeOffset.UtcNow;
    }

    public Guid DeviceId { get; private set; }
    public Guid? PaymentId { get; private set; }
    public string TransactionType { get; private set; } = string.Empty;
    public decimal? Amount { get; private set; }
    public string Status { get; private set; } = string.Empty;
    public string? CardNumber { get; private set; }
    public string? ApprovalCode { get; private set; }
    public string? TerminalId { get; private set; }
    public string? MerchantId { get; private set; }
    public string? TransactionData { get; private set; }
    public DateTimeOffset Timestamp { get; private set; }
    
    // Navigation properties
    public IoTDevice Device { get; private set; } = null!;
    public Payment? Payment { get; private set; }

    public static DeviceTransaction Create(Guid deviceId, string transactionType, string status)
    {
        return new DeviceTransaction(Guid.NewGuid(), deviceId, transactionType, status);
    }

    public void LinkPayment(Guid paymentId)
    {
        PaymentId = paymentId;
    }

    public void UpdateAmount(decimal amount)
    {
        Amount = amount;
    }

    public void UpdateCardInfo(string? cardNumber, string? approvalCode)
    {
        CardNumber = cardNumber;
        ApprovalCode = approvalCode;
    }

    public void UpdateTerminalInfo(string? terminalId, string? merchantId)
    {
        TerminalId = terminalId;
        MerchantId = merchantId;
    }

    public void AddTransactionData(string? transactionData)
    {
        TransactionData = transactionData;
    }

    public void Complete(string? approvalCode)
    {
        Status = "completed";
        ApprovalCode = approvalCode;
    }

    public void Fail()
    {
        Status = "failed";
    }
}