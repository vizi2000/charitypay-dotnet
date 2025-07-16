using CharityPay.Application.Abstractions.Repositories;
using CharityPay.Domain.Entities;
using CharityPay.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CharityPay.Infrastructure.Data.Repositories;

/// <summary>
/// Repository implementation for refresh tokens.
/// </summary>
public class RefreshTokenRepository : GenericRepository<RefreshToken>, IRefreshTokenRepository
{
    public RefreshTokenRepository(CharityPayDbContext context) : base(context)
    {
    }
    
    public async Task<RefreshToken?> GetByTokenAsync(string token, CancellationToken cancellationToken = default)
    {
        return await _context.Set<RefreshToken>()
            .Include(rt => rt.User)
            .FirstOrDefaultAsync(rt => rt.Token == token, cancellationToken);
    }
    
    public async Task<IEnumerable<RefreshToken>> GetValidTokensByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _context.Set<RefreshToken>()
            .Where(rt => rt.UserId == userId && !rt.IsRevoked && rt.ExpiresAt > DateTime.UtcNow)
            .ToListAsync(cancellationToken);
    }
    
    public async Task RevokeAllUserTokensAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var tokens = await _context.Set<RefreshToken>()
            .Where(rt => rt.UserId == userId && !rt.IsRevoked)
            .ToListAsync(cancellationToken);
            
        foreach (var token in tokens)
        {
            token.Revoke();
        }
    }
    
    public async Task<int> RemoveExpiredTokensAsync(CancellationToken cancellationToken = default)
    {
        var expiredTokens = await _context.Set<RefreshToken>()
            .Where(rt => rt.ExpiresAt <= DateTime.UtcNow)
            .ToListAsync(cancellationToken);
            
        _context.Set<RefreshToken>().RemoveRange(expiredTokens);
        return expiredTokens.Count;
    }
}