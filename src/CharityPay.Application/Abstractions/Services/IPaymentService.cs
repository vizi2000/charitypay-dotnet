using CharityPay.Application.Common.Models;
using CharityPay.Application.DTOs.Payment;
using CharityPay.Domain.Enums;

namespace CharityPay.Application.Abstractions.Services;

/// <summary>
/// Payment processing service interface.
/// </summary>
public interface IPaymentService
{
    /// <summary>
    /// Initiates a new payment.
    /// </summary>
    Task<InitiatePaymentResponse> InitiatePaymentAsync(
        InitiatePaymentRequest request, 
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets payment status by ID.
    /// </summary>
    Task<PaymentDto?> GetPaymentStatusAsync(Guid paymentId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Cancels a pending payment.
    /// </summary>
    Task<bool> CancelPaymentAsync(Guid paymentId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets payments for an organization.
    /// </summary>
    Task<PaginatedResponse<PaymentDto>> GetOrganizationPaymentsAsync(
        Guid organizationId, 
        int page = 1, 
        int pageSize = 10,
        PaymentStatus? status = null,
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Processes a payment webhook from Fiserv.
    /// </summary>
    Task ProcessWebhookAsync(string payload, string signature, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets payment statistics for an organization.
    /// </summary>
    Task<(decimal TotalAmount, int TotalCount, int CompletedCount)> GetPaymentStatisticsAsync(
        Guid organizationId, 
        DateTime fromDate, 
        DateTime toDate,
        CancellationToken cancellationToken = default);
}