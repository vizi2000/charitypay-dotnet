using CharityPay.Domain.Enums;

namespace CharityPay.Application.DTOs.Payment;

/// <summary>
/// Request model for initiating a payment.
/// </summary>
public class InitiatePaymentRequest
{
    /// <summary>
    /// Organization ID to donate to.
    /// </summary>
    public Guid OrganizationId { get; set; }
    
    /// <summary>
    /// Donation amount.
    /// </summary>
    public decimal Amount { get; set; }
    
    /// <summary>
    /// Payment method.
    /// </summary>
    public PaymentMethod PaymentMethod { get; set; }
    
    /// <summary>
    /// Donor name (required).
    /// </summary>
    public string DonorName { get; set; } = string.Empty;
    
    /// <summary>
    /// Donor email (optional, for receipt).
    /// </summary>
    public string? DonorEmail { get; set; }
    
    /// <summary>
    /// Donor phone (optional).
    /// </summary>
    public string? DonorPhone { get; set; }
    
    /// <summary>
    /// Return URL after payment completion.
    /// </summary>
    public string ReturnUrl { get; set; } = string.Empty;
}