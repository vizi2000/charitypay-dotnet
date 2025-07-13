using CharityPay.Domain.Enums;
using CharityPay.Domain.Shared;

namespace CharityPay.Domain.Entities;

public sealed class User : Entity
{
    private User() { } // For EF Core

    private User(Guid id, string email, string passwordHash, string fullName, UserRole role)
        : base(id)
    {
        Email = email;
        PasswordHash = passwordHash;
        FullName = fullName;
        Role = role;
        IsActive = true;
        CreatedAt = DateTimeOffset.UtcNow;
    }

    public string Email { get; private set; } = string.Empty;
    public string PasswordHash { get; private set; } = string.Empty;
    public string FullName { get; private set; } = string.Empty;
    public UserRole Role { get; private set; }
    public bool IsActive { get; private set; }
    public DateTimeOffset? LastLogin { get; private set; }
    
    // Navigation properties
    public Organization? Organization { get; private set; }

    public static User Create(string email, string passwordHash, string fullName, UserRole role)
    {
        return new User(Guid.NewGuid(), email, passwordHash, fullName, role);
    }

    public void UpdateLastLogin()
    {
        LastLogin = DateTimeOffset.UtcNow;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void Activate()
    {
        IsActive = true;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void Deactivate()
    {
        IsActive = false;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void UpdatePassword(string newPasswordHash)
    {
        PasswordHash = newPasswordHash;
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}