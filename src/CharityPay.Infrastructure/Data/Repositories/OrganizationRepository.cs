using Microsoft.EntityFrameworkCore;
using CharityPay.Domain.Entities;
using CharityPay.Domain.Enums;
using CharityPay.Application.Abstractions.Repositories;

namespace CharityPay.Infrastructure.Data.Repositories;

public class OrganizationRepository : GenericRepository<Organization>, IOrganizationRepository
{
    public OrganizationRepository(CharityPayDbContext context) : base(context)
    {
    }

    public async Task<Organization?> GetByIdWithUserAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(o => o.User)
            .FirstOrDefaultAsync(o => o.Id == id, cancellationToken);
    }

    public async Task<Organization?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .FirstOrDefaultAsync(o => o.UserId == userId, cancellationToken);
    }

    public async Task<IEnumerable<Organization>> GetApprovedOrganizationsAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(o => o.Status == OrganizationStatus.Active)
            .OrderBy(o => o.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Organization>> GetByStatusAsync(OrganizationStatus status, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(o => o.Status == status)
            .OrderBy(o => o.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Organization>> GetByCategoryAsync(string category, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(o => o.Category == category && o.Status == OrganizationStatus.Active)
            .OrderBy(o => o.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Organization>> GetByLocationAsync(string location, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(o => o.Location == location && o.Status == OrganizationStatus.Active)
            .OrderBy(o => o.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<(IEnumerable<Organization> Organizations, int TotalCount)> GetPagedAsync(
        string? category = null,
        string? location = null,
        OrganizationStatus? status = null,
        int skip = 0,
        int take = 10,
        CancellationToken cancellationToken = default)
    {
        var query = _dbSet.AsQueryable();

        if (!string.IsNullOrEmpty(category))
        {
            query = query.Where(o => o.Category == category);
        }

        if (!string.IsNullOrEmpty(location))
        {
            query = query.Where(o => o.Location == location);
        }

        if (status.HasValue)
        {
            query = query.Where(o => o.Status == status.Value);
        }
        else
        {
            query = query.Where(o => o.Status == OrganizationStatus.Active);
        }

        var totalCount = await query.CountAsync(cancellationToken);
        var organizations = await query
            .OrderBy(o => o.Name)
            .Skip(skip)
            .Take(take)
            .ToListAsync(cancellationToken);

        return (organizations, totalCount);
    }

    public async Task<bool> ExistsByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AnyAsync(o => o.Name == name, cancellationToken);
    }

    public async Task<decimal> GetTotalCollectedAmountAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(o => o.Status == OrganizationStatus.Active)
            .SumAsync(o => o.CollectedAmount, cancellationToken);
    }

    public async Task<int> GetCountByStatusAsync(OrganizationStatus status, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .CountAsync(o => o.Status == status, cancellationToken);
    }

    public async Task UpdateCollectedAmountAsync(Guid organizationId, decimal amount, CancellationToken cancellationToken = default)
    {
        var organization = await _dbSet.FindAsync(new object[] { organizationId }, cancellationToken);
        if (organization != null)
        {
            organization.UpdateCollectedAmount(amount);
            _dbSet.Update(organization);
        }
    }

    public async Task<(IEnumerable<Organization> Organizations, int TotalCount)> GetApprovedPaginatedAsync(
        int page, 
        int pageSize, 
        CancellationToken cancellationToken = default)
    {
        var skip = (page - 1) * pageSize;
        var query = _dbSet.Where(o => o.Status == OrganizationStatus.Active);
        
        var totalCount = await query.CountAsync(cancellationToken);
        var organizations = await query
            .OrderBy(o => o.Name)
            .Skip(skip)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (organizations, totalCount);
    }

    public async Task<IEnumerable<Organization>> SearchByNameAsync(string searchTerm, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(o => o.Name.Contains(searchTerm) && o.Status == OrganizationStatus.Active)
            .OrderBy(o => o.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<OrganizationAnalytics?> GetAnalyticsAsync(Guid organizationId, CancellationToken cancellationToken = default)
    {
        return await _context.OrganizationAnalytics
            .FirstOrDefaultAsync(a => a.OrganizationId == organizationId, cancellationToken);
    }

    public async Task<Organization?> GetByPolcardMerchantIdAsync(string polcardMerchantId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(o => o.User)
            .Include(o => o.Documents)
            .FirstOrDefaultAsync(o => o.PolcardMerchantId == polcardMerchantId, cancellationToken);
    }
}