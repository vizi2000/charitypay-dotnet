using CharityPay.Domain.Entities;
using CharityPay.Domain.Enums;
using CharityPay.Domain.Shared;
using FluentAssertions;
using Xunit;

namespace CharityPay.Domain.Tests.Entities;

public class UserEntityTests
{
    [Fact]
    public void User_Create_ShouldCreateWithValidData()
    {
        // Arrange
        var email = "test@example.com";
        var passwordHash = "hashedPassword123";
        var fullName = "John Doe";
        var role = UserRole.Organization;

        // Act
        var user = User.Create(email, passwordHash, fullName, role);

        // Assert
        user.Should().NotBeNull();
        user.Id.Should().NotBe(Guid.Empty);
        user.Email.Should().Be(email);
        user.PasswordHash.Should().Be(passwordHash);
        user.FullName.Should().Be(fullName);
        user.Role.Should().Be(role);
        user.IsActive.Should().BeTrue();
        user.LastLogin.Should().BeNull();
        user.CreatedAt.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromMinutes(1));
    }

    [Theory]
    [InlineData(UserRole.Admin)]
    [InlineData(UserRole.Organization)]
    public void User_Create_ShouldAllowValidRoles(UserRole role)
    {
        // Arrange
        var email = "test@example.com";
        var passwordHash = "hashedPassword123";
        var fullName = "Test User";

        // Act
        var user = User.Create(email, passwordHash, fullName, role);

        // Assert
        user.Role.Should().Be(role);
    }

    [Fact]
    public void User_ShouldInheritFromEntity()
    {
        // Arrange
        var user = User.Create("test@example.com", "hash", "Test User", UserRole.Organization);

        // Assert
        user.Should().BeAssignableTo<Entity>();
    }

    [Fact]
    public void User_UpdateLastLogin_ShouldSetLastLoginTime()
    {
        // Arrange
        var user = User.Create("test@example.com", "hash", "Test User", UserRole.Organization);
        user.LastLogin.Should().BeNull();

        // Act
        user.UpdateLastLogin();

        // Assert
        user.LastLogin.Should().NotBeNull();
        user.LastLogin.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromMinutes(1));
        user.UpdatedAt.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromMinutes(1));
    }

    [Fact]
    public void User_Activate_ShouldSetIsActiveToTrue()
    {
        // Arrange
        var user = User.Create("test@example.com", "hash", "Test User", UserRole.Organization);
        // User is active by default, so let's deactivate first
        user.Deactivate();
        user.IsActive.Should().BeFalse();

        // Act
        user.Activate();

        // Assert
        user.IsActive.Should().BeTrue();
        user.UpdatedAt.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromMinutes(1));
    }

    [Fact]
    public void User_Deactivate_ShouldSetIsActiveToFalse()
    {
        // Arrange
        var user = User.Create("test@example.com", "hash", "Test User", UserRole.Organization);
        user.IsActive.Should().BeTrue();

        // Act
        user.Deactivate();

        // Assert
        user.IsActive.Should().BeFalse();
        user.UpdatedAt.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromMinutes(1));
    }

    [Fact]
    public void User_UpdatePassword_ShouldChangePasswordHash()
    {
        // Arrange
        var user = User.Create("test@example.com", "oldHash", "Test User", UserRole.Organization);
        var newPasswordHash = "newHashedPassword456";

        // Act
        user.UpdatePassword(newPasswordHash);

        // Assert
        user.PasswordHash.Should().Be(newPasswordHash);
        user.UpdatedAt.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromMinutes(1));
    }

    [Fact]
    public void User_DefaultValues_ShouldBeCorrect()
    {
        // Act
        var user = User.Create("test@example.com", "hash", "Test User", UserRole.Organization);

        // Assert
        user.IsActive.Should().BeTrue();
        user.LastLogin.Should().BeNull();
        user.Organization.Should().BeNull();
    }

    [Theory]
    [InlineData("admin@test.com", "AdminUser", UserRole.Admin)]
    [InlineData("org@test.com", "OrgUser", UserRole.Organization)]
    public void User_Create_WithDifferentRoles_ShouldWork(string email, string fullName, UserRole role)
    {
        // Act
        var user = User.Create(email, "hashedPassword", fullName, role);

        // Assert
        user.Email.Should().Be(email);
        user.FullName.Should().Be(fullName);
        user.Role.Should().Be(role);
    }

    [Fact]
    public void User_CreatedAt_ShouldBeSetOnCreation()
    {
        // Arrange
        var beforeCreation = DateTimeOffset.UtcNow;

        // Act
        var user = User.Create("test@example.com", "hash", "Test User", UserRole.Organization);

        // Assert
        var afterCreation = DateTimeOffset.UtcNow;
        user.CreatedAt.Should().BeOnOrAfter(beforeCreation);
        user.CreatedAt.Should().BeOnOrBefore(afterCreation);
    }

    [Fact]
    public void User_UpdatedAt_ShouldBeUpdatedOnPasswordChange()
    {
        // Arrange
        var user = User.Create("test@example.com", "hash", "Test User", UserRole.Organization);
        var originalUpdatedAt = user.UpdatedAt;
        
        // Wait a moment to ensure time difference
        Thread.Sleep(1);

        // Act
        user.UpdatePassword("newHash");

        // Assert
        user.UpdatedAt.Should().BeAfter(originalUpdatedAt);
    }
}