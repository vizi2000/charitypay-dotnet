using CharityPay.Domain.Entities;

namespace CharityPay.Application.Abstractions.Repositories;

/// <summary>
/// Repository interface for refresh tokens.
/// </summary>
public interface IRefreshTokenRepository : IGenericRepository<RefreshToken>
{
    /// <summary>
    /// Gets a refresh token by its token value.
    /// </summary>
    Task<RefreshToken?> GetByTokenAsync(string token, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets all valid refresh tokens for a user.
    /// </summary>
    Task<IEnumerable<RefreshToken>> GetValidTokensByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Revokes all refresh tokens for a user.
    /// </summary>
    Task RevokeAllUserTokensAsync(Guid userId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Removes expired tokens from the database.
    /// </summary>
    Task<int> RemoveExpiredTokensAsync(CancellationToken cancellationToken = default);
}