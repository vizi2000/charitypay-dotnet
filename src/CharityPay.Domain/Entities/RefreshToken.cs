using CharityPay.Domain.Shared;

namespace CharityPay.Domain.Entities;

/// <summary>
/// Represents a refresh token for JWT authentication.
/// </summary>
public class RefreshToken : Entity
{
    /// <summary>
    /// Gets the token value.
    /// </summary>
    public string Token { get; private set; } = string.Empty;
    
    /// <summary>
    /// Gets the user ID associated with this token.
    /// </summary>
    public Guid UserId { get; private set; }
    
    /// <summary>
    /// Gets the expiration date of the token.
    /// </summary>
    public DateTime ExpiresAt { get; private set; }
    
    /// <summary>
    /// Gets whether the token has been revoked.
    /// </summary>
    public bool IsRevoked { get; private set; }
    
    /// <summary>
    /// Gets the date when the token was revoked.
    /// </summary>
    public DateTime? RevokedAt { get; private set; }
    
    /// <summary>
    /// Navigation property to the user.
    /// </summary>
    public virtual User User { get; private set; } = null!;
    
    /// <summary>
    /// Private constructor for EF Core.
    /// </summary>
    private RefreshToken() { }
    
    /// <summary>
    /// Creates a new refresh token.
    /// </summary>
    public static RefreshToken Create(string token, Guid userId, int expirationDays)
    {
        if (string.IsNullOrWhiteSpace(token))
            throw new ArgumentException("Token cannot be empty", nameof(token));
            
        if (userId == Guid.Empty)
            throw new ArgumentException("User ID cannot be empty", nameof(userId));
            
        if (expirationDays <= 0)
            throw new ArgumentException("Expiration days must be positive", nameof(expirationDays));
        
        return new RefreshToken
        {
            Id = Guid.NewGuid(),
            Token = token,
            UserId = userId,
            ExpiresAt = DateTime.UtcNow.AddDays(expirationDays),
            IsRevoked = false,
            CreatedAt = DateTime.UtcNow
        };
    }
    
    /// <summary>
    /// Revokes the refresh token.
    /// </summary>
    public void Revoke()
    {
        if (IsRevoked)
            throw new InvalidOperationException("Token is already revoked");
            
        IsRevoked = true;
        RevokedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }
    
    /// <summary>
    /// Checks if the token is valid (not expired and not revoked).
    /// </summary>
    public bool IsValid => !IsRevoked && ExpiresAt > DateTime.UtcNow;
}