using CharityPay.Domain.Entities;

namespace CharityPay.Application.Abstractions.Services;

/// <summary>
/// Service for managing refresh tokens.
/// </summary>
public interface IRefreshTokenService
{
    /// <summary>
    /// Creates and stores a new refresh token for a user.
    /// </summary>
    Task<string> CreateRefreshTokenAsync(Guid userId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Validates a refresh token and returns the associated user.
    /// </summary>
    Task<User?> ValidateRefreshTokenAsync(string token, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Revokes a specific refresh token.
    /// </summary>
    Task RevokeRefreshTokenAsync(string token, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Revokes all refresh tokens for a user.
    /// </summary>
    Task RevokeAllUserTokensAsync(Guid userId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Cleans up expired tokens from the database.
    /// </summary>
    Task<int> CleanupExpiredTokensAsync(CancellationToken cancellationToken = default);
}