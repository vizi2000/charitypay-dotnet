namespace CharityPay.Application.DTOs.Payment;

/// <summary>
/// Response model for payment initiation.
/// </summary>
public class InitiatePaymentResponse
{
    /// <summary>
    /// The payment ID.
    /// </summary>
    public Guid PaymentId { get; set; }
    
    /// <summary>
    /// The payment redirect URL (Fiserv checkout page).
    /// </summary>
    public string PaymentUrl { get; set; } = string.Empty;
    
    /// <summary>
    /// The Fiserv order ID for tracking.
    /// </summary>
    public string OrderId { get; set; } = string.Empty;
    
    /// <summary>
    /// Expiration time for the payment link.
    /// </summary>
    public DateTimeOffset ExpiresAt { get; set; }
}