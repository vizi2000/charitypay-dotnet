using CharityPay.Domain.Entities;
using CharityPay.Domain.Enums;

namespace CharityPay.Application.Abstractions.Repositories;

/// <summary>
/// Repository interface for Organization entity operations.
/// </summary>
public interface IOrganizationRepository : IGenericRepository<Organization>
{
    /// <summary>
    /// Gets organizations by status.
    /// </summary>
    /// <param name="status">The organization status.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Collection of organizations with the specified status.</returns>
    Task<IEnumerable<Organization>> GetByStatusAsync(OrganizationStatus status, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets approved organizations with pagination.
    /// </summary>
    /// <param name="page">Page number (1-based).</param>
    /// <param name="pageSize">Number of items per page.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Paginated collection of approved organizations.</returns>
    Task<(IEnumerable<Organization> Organizations, int TotalCount)> GetApprovedPaginatedAsync(
        int page, 
        int pageSize, 
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets an organization by its user ID.
    /// </summary>
    /// <param name="userId">The user identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The organization or null if not found.</returns>
    Task<Organization?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Searches organizations by name.
    /// </summary>
    /// <param name="searchTerm">The search term.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Collection of organizations matching the search.</returns>
    Task<IEnumerable<Organization>> SearchByNameAsync(string searchTerm, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets organization statistics.
    /// </summary>
    /// <param name="organizationId">The organization identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Organization statistics or null if not found.</returns>
    Task<OrganizationAnalytics?> GetAnalyticsAsync(Guid organizationId, CancellationToken cancellationToken = default);
}