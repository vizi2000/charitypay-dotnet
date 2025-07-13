using CharityPay.Domain.Enums;

namespace CharityPay.Application.DTOs.Payment;

/// <summary>
/// Payment data transfer object.
/// </summary>
public class PaymentDto
{
    /// <summary>
    /// Payment ID.
    /// </summary>
    public Guid Id { get; set; }
    
    /// <summary>
    /// Organization ID.
    /// </summary>
    public Guid OrganizationId { get; set; }
    
    /// <summary>
    /// Organization name.
    /// </summary>
    public string OrganizationName { get; set; } = string.Empty;
    
    /// <summary>
    /// Payment amount.
    /// </summary>
    public decimal Amount { get; set; }
    
    /// <summary>
    /// Payment status.
    /// </summary>
    public PaymentStatus Status { get; set; }
    
    /// <summary>
    /// Payment method.
    /// </summary>
    public PaymentMethod PaymentMethod { get; set; }
    
    /// <summary>
    /// Donor name (may be anonymous).
    /// </summary>
    public string DonorName { get; set; } = string.Empty;
    
    /// <summary>
    /// Donor email (optional).
    /// </summary>
    public string? DonorEmail { get; set; }
    
    /// <summary>
    /// Donor phone (optional).
    /// </summary>
    public string? DonorPhone { get; set; }
    
    /// <summary>
    /// Fiserv order ID.
    /// </summary>
    public string? FiservOrderId { get; set; }
    
    /// <summary>
    /// Payment creation timestamp.
    /// </summary>
    public DateTimeOffset CreatedAt { get; set; }
    
    /// <summary>
    /// Payment completion timestamp.
    /// </summary>
    public DateTimeOffset? CompletedAt { get; set; }
}