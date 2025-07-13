using CharityPay.Application.Common.Models;
using CharityPay.Application.DTOs.Organization;

namespace CharityPay.Application.Abstractions.Services;

/// <summary>
/// Organization management service interface.
/// </summary>
public interface IOrganizationService
{
    /// <summary>
    /// Gets a paginated list of approved organizations.
    /// </summary>
    Task<PaginatedResponse<OrganizationDto>> GetOrganizationsAsync(
        int page = 1, 
        int pageSize = 10, 
        string? category = null, 
        string? location = null,
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets an organization by ID.
    /// </summary>
    Task<OrganizationDto?> GetOrganizationByIdAsync(Guid id, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets the current user's organization profile.
    /// </summary>
    Task<OrganizationDto?> GetMyOrganizationAsync(Guid userId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Updates the current user's organization profile.
    /// </summary>
    Task<OrganizationDto> UpdateOrganizationAsync(
        Guid userId, 
        UpdateOrganizationRequest request, 
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Updates the organization's logo.
    /// </summary>
    Task<string> UpdateOrganizationLogoAsync(
        Guid userId, 
        Stream logoStream, 
        string fileName,
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets organization statistics.
    /// </summary>
    Task<OrganizationStatsDto> GetOrganizationStatsAsync(
        Guid organizationId, 
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Searches organizations by name.
    /// </summary>
    Task<IEnumerable<OrganizationDto>> SearchOrganizationsAsync(
        string searchTerm, 
        CancellationToken cancellationToken = default);
}