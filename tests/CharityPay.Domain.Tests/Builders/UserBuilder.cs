using CharityPay.Domain.Entities;
using CharityPay.Domain.Enums;

namespace CharityPay.Domain.Tests.Builders;

/// <summary>
/// Simplified builder for <see cref="User"/> that utilises the factory methods
/// provided by the domain model. Older implementations set properties directly
/// which is incompatible with the current private setters.
/// </summary>
public sealed class UserBuilder
{
    private string _email = $"test{Guid.NewGuid():N}@example.com";
    private string _passwordHash = "hashed_password";
    private string _fullName = "Test User";
    private UserRole _role = UserRole.Organization;
    private bool _inactive;

    public UserBuilder WithEmail(string email)
    {
        _email = email;
        return this;
    }

    public UserBuilder WithPasswordHash(string hash)
    {
        _passwordHash = hash;
        return this;
    }

    public UserBuilder WithFullName(string name)
    {
        _fullName = name;
        return this;
    }

    public UserBuilder AsAdmin()
    {
        _role = UserRole.Admin;
        return this;
    }

    public UserBuilder AsInactive()
    {
        _inactive = true;
        return this;
    }

    public User Build()
    {
        var user = User.Create(_email, _passwordHash, _fullName, _role);
        if (_inactive)
        {
            user.Deactivate();
        }
        return user;
    }

    public List<User> BuildMany(int count)
    {
        var list = new List<User>();
        for (int i = 0; i < count; i++)
        {
            list.Add(new UserBuilder()
                .WithEmail(_email)
                .WithPasswordHash(_passwordHash)
                .WithFullName(_fullName)
                .Build());
        }
        return list;
    }
}
