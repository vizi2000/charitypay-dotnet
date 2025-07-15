using CharityPay.Domain.Entities;
using CharityPay.Domain.Enums;

namespace CharityPay.Domain.Tests.Builders;

public class UserBuilder
{
    private User _user;

    public UserBuilder()
    {
        _user = new User
        {
            Id = Guid.NewGuid(),
            Email = $"test{Guid.NewGuid():N}@example.com",
            FirstName = "Test",
            LastName = "User",
            Role = UserRole.Organization,
            PasswordHash = "hashed_password",
            IsActive = true,
            EmailConfirmed = true,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };
    }

    public UserBuilder WithId(Guid id)
    {
        _user.Id = id;
        return this;
    }

    public UserBuilder WithEmail(string email)
    {
        _user.Email = email;
        return this;
    }

    public UserBuilder WithName(string firstName, string lastName)
    {
        _user.FirstName = firstName;
        _user.LastName = lastName;
        return this;
    }

    public UserBuilder AsAdmin()
    {
        _user.Role = UserRole.Admin;
        _user.Email = $"admin{Guid.NewGuid():N}@example.com";
        return this;
    }

    public UserBuilder AsOrganization()
    {
        _user.Role = UserRole.Organization;
        return this;
    }

    public UserBuilder AsInactive()
    {
        _user.IsActive = false;
        return this;
    }

    public User Build() => _user;
    
    public List<User> BuildMany(int count)
    {
        var users = new List<User>();
        for (int i = 0; i < count; i++)
        {
            users.Add(new UserBuilder().Build());
        }
        return users;
    }
}