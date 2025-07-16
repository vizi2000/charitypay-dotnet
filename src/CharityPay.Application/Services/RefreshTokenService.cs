using System.Security.Cryptography;
using CharityPay.Application.Abstractions;
using CharityPay.Application.Abstractions.Services;
using CharityPay.Application.Configuration;
using CharityPay.Domain.Entities;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CharityPay.Application.Services;

/// <summary>
/// Service implementation for managing refresh tokens.
/// </summary>
public class RefreshTokenService : IRefreshTokenService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly JwtSettings _jwtSettings;
    private readonly ILogger<RefreshTokenService> _logger;
    
    public RefreshTokenService(
        IUnitOfWork unitOfWork,
        IOptions<JwtSettings> jwtSettings,
        ILogger<RefreshTokenService> logger)
    {
        _unitOfWork = unitOfWork;
        _jwtSettings = jwtSettings.Value;
        _logger = logger;
    }
    
    public async Task<string> CreateRefreshTokenAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var tokenValue = GenerateSecureToken();
        var refreshToken = RefreshToken.Create(tokenValue, userId, _jwtSettings.RefreshTokenExpirationDays);
        
        await _unitOfWork.RefreshTokens.AddAsync(refreshToken, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        
        _logger.LogInformation("Created refresh token for user {UserId}", userId);
        
        return tokenValue;
    }
    
    public async Task<User?> ValidateRefreshTokenAsync(string token, CancellationToken cancellationToken = default)
    {
        var refreshToken = await _unitOfWork.RefreshTokens.GetByTokenAsync(token, cancellationToken);
        
        if (refreshToken == null || !refreshToken.IsValid)
        {
            _logger.LogWarning("Invalid refresh token validation attempt");
            return null;
        }
        
        return refreshToken.User;
    }
    
    public async Task RevokeRefreshTokenAsync(string token, CancellationToken cancellationToken = default)
    {
        var refreshToken = await _unitOfWork.RefreshTokens.GetByTokenAsync(token, cancellationToken);
        
        if (refreshToken != null && !refreshToken.IsRevoked)
        {
            refreshToken.Revoke();
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Revoked refresh token for user {UserId}", refreshToken.UserId);
        }
    }
    
    public async Task RevokeAllUserTokensAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        await _unitOfWork.RefreshTokens.RevokeAllUserTokensAsync(userId, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Revoked all refresh tokens for user {UserId}", userId);
    }
    
    public async Task<int> CleanupExpiredTokensAsync(CancellationToken cancellationToken = default)
    {
        var count = await _unitOfWork.RefreshTokens.RemoveExpiredTokensAsync(cancellationToken);
        if (count > 0)
        {
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Cleaned up {Count} expired refresh tokens", count);
        }
        return count;
    }
    
    private static string GenerateSecureToken()
    {
        var randomNumber = new byte[32];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);
        return Convert.ToBase64String(randomNumber);
    }
}