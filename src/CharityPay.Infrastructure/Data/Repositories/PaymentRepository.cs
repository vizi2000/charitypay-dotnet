using Microsoft.EntityFrameworkCore;
using CharityPay.Domain.Entities;
using CharityPay.Domain.Enums;
using CharityPay.Application.Abstractions.Repositories;

namespace CharityPay.Infrastructure.Data.Repositories;

public class PaymentRepository : GenericRepository<Payment>, IPaymentRepository
{
    public PaymentRepository(CharityPayDbContext context) : base(context)
    {
    }

    public async Task<Payment?> GetByIdWithOrganizationAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(p => p.Organization)
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
    }

    public async Task<Payment?> GetByFiservOrderIdAsync(string fiservOrderId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .FirstOrDefaultAsync(p => p.FiservOrderId == fiservOrderId, cancellationToken);
    }

    public async Task<IEnumerable<Payment>> GetByOrganizationIdAsync(Guid organizationId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(p => p.OrganizationId == organizationId)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Payment>> GetByStatusAsync(PaymentStatus status, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(p => p.Status == status)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Payment>> GetRecentPaymentsAsync(int count = 10, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(p => p.Organization)
            .OrderByDescending(p => p.CreatedAt)
            .Take(count)
            .ToListAsync(cancellationToken);
    }

    public async Task<(IEnumerable<Payment> Payments, int TotalCount)> GetPagedByOrganizationAsync(
        Guid organizationId,
        PaymentStatus? status = null,
        int skip = 0,
        int take = 10,
        CancellationToken cancellationToken = default)
    {
        var query = _dbSet.Where(p => p.OrganizationId == organizationId);

        if (status.HasValue)
        {
            query = query.Where(p => p.Status == status.Value);
        }

        var totalCount = await query.CountAsync(cancellationToken);
        var payments = await query
            .OrderByDescending(p => p.CreatedAt)
            .Skip(skip)
            .Take(take)
            .ToListAsync(cancellationToken);

        return (payments, totalCount);
    }

    public async Task<(IEnumerable<Payment> Payments, int TotalCount)> GetPagedAsync(
        PaymentStatus? status = null,
        Guid? organizationId = null,
        int skip = 0,
        int take = 10,
        CancellationToken cancellationToken = default)
    {
        var query = _dbSet.Include(p => p.Organization).AsQueryable();

        if (status.HasValue)
        {
            query = query.Where(p => p.Status == status.Value);
        }

        if (organizationId.HasValue)
        {
            query = query.Where(p => p.OrganizationId == organizationId.Value);
        }

        var totalCount = await query.CountAsync(cancellationToken);
        var payments = await query
            .OrderByDescending(p => p.CreatedAt)
            .Skip(skip)
            .Take(take)
            .ToListAsync(cancellationToken);

        return (payments, totalCount);
    }

    public async Task<decimal> GetTotalAmountByOrganizationAsync(Guid organizationId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(p => p.OrganizationId == organizationId && p.Status == PaymentStatus.Completed)
            .SumAsync(p => p.Amount, cancellationToken);
    }

    public async Task<int> GetCountByOrganizationAsync(Guid organizationId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .CountAsync(p => p.OrganizationId == organizationId && p.Status == PaymentStatus.Completed, cancellationToken);
    }

    public async Task<decimal> GetAverageAmountByOrganizationAsync(Guid organizationId, CancellationToken cancellationToken = default)
    {
        var completedPayments = await _dbSet
            .Where(p => p.OrganizationId == organizationId && p.Status == PaymentStatus.Completed)
            .ToListAsync(cancellationToken);

        return completedPayments.Any() ? completedPayments.Average(p => p.Amount) : 0;
    }

    public async Task<Payment?> GetLastPaymentByOrganizationAsync(Guid organizationId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(p => p.OrganizationId == organizationId && p.Status == PaymentStatus.Completed)
            .OrderByDescending(p => p.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<IEnumerable<Payment>> GetPaymentsByDateRangeAsync(
        DateTimeOffset startDate,
        DateTimeOffset endDate,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(p => p.CreatedAt >= startDate && p.CreatedAt <= endDate)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<decimal> GetTotalAmountAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(p => p.Status == PaymentStatus.Completed)
            .SumAsync(p => p.Amount, cancellationToken);
    }

    public async Task<int> GetTotalCountAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .CountAsync(p => p.Status == PaymentStatus.Completed, cancellationToken);
    }

    public async Task<IEnumerable<Payment>> GetTodaysPaymentsByOrganizationAsync(Guid organizationId, CancellationToken cancellationToken = default)
    {
        var today = DateTimeOffset.UtcNow.Date;
        var tomorrow = today.AddDays(1);

        return await _dbSet
            .Where(p => p.OrganizationId == organizationId && 
                       p.CreatedAt >= today && 
                       p.CreatedAt < tomorrow)
            .ToListAsync(cancellationToken);
    }

    public async Task<(IEnumerable<Payment> Payments, int TotalCount)> GetByOrganizationPaginatedAsync(
        Guid organizationId, 
        int page, 
        int pageSize, 
        CancellationToken cancellationToken = default)
    {
        var skip = (page - 1) * pageSize;
        var query = _dbSet.Where(p => p.OrganizationId == organizationId);
        
        var totalCount = await query.CountAsync(cancellationToken);
        var payments = await query
            .OrderByDescending(p => p.CreatedAt)
            .Skip(skip)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (payments, totalCount);
    }

    public async Task<Payment?> GetByExternalTransactionIdAsync(string externalTransactionId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .FirstOrDefaultAsync(p => p.FiservOrderId == externalTransactionId, cancellationToken);
    }

    public async Task<(decimal TotalAmount, int TotalCount, int CompletedCount)> GetStatisticsAsync(
        Guid organizationId, 
        DateTime fromDate, 
        DateTime toDate, 
        CancellationToken cancellationToken = default)
    {
        var payments = await _dbSet
            .Where(p => p.OrganizationId == organizationId && 
                       p.CreatedAt >= fromDate && 
                       p.CreatedAt <= toDate)
            .ToListAsync(cancellationToken);

        var totalAmount = payments.Where(p => p.Status == PaymentStatus.Completed).Sum(p => p.Amount);
        var totalCount = payments.Count;
        var completedCount = payments.Count(p => p.Status == PaymentStatus.Completed);

        return (totalAmount, totalCount, completedCount);
    }
}