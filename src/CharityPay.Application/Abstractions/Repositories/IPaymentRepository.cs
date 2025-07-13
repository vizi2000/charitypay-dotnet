using CharityPay.Domain.Entities;
using CharityPay.Domain.Enums;

namespace CharityPay.Application.Abstractions.Repositories;

/// <summary>
/// Repository interface for Payment entity operations.
/// </summary>
public interface IPaymentRepository : IGenericRepository<Payment>
{
    /// <summary>
    /// Gets payments for a specific organization.
    /// </summary>
    /// <param name="organizationId">The organization identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Collection of payments for the organization.</returns>
    Task<IEnumerable<Payment>> GetByOrganizationIdAsync(Guid organizationId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets payments by status.
    /// </summary>
    /// <param name="status">The payment status.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Collection of payments with the specified status.</returns>
    Task<IEnumerable<Payment>> GetByStatusAsync(PaymentStatus status, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets payments for an organization with pagination.
    /// </summary>
    /// <param name="organizationId">The organization identifier.</param>
    /// <param name="page">Page number (1-based).</param>
    /// <param name="pageSize">Number of items per page.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Paginated collection of payments.</returns>
    Task<(IEnumerable<Payment> Payments, int TotalCount)> GetByOrganizationPaginatedAsync(
        Guid organizationId, 
        int page, 
        int pageSize, 
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets a payment by external transaction ID.
    /// </summary>
    /// <param name="externalTransactionId">The external transaction identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The payment or null if not found.</returns>
    Task<Payment?> GetByExternalTransactionIdAsync(string externalTransactionId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets payment statistics for an organization.
    /// </summary>
    /// <param name="organizationId">The organization identifier.</param>
    /// <param name="fromDate">Start date for statistics.</param>
    /// <param name="toDate">End date for statistics.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Payment statistics.</returns>
    Task<(decimal TotalAmount, int TotalCount, int CompletedCount)> GetStatisticsAsync(
        Guid organizationId, 
        DateTime fromDate, 
        DateTime toDate, 
        CancellationToken cancellationToken = default);
}