using CharityPay.Domain.Entities;

namespace CharityPay.Application.Abstractions.Repositories;

/// <summary>
/// Repository interface for User entity operations.
/// </summary>
public interface IUserRepository : IGenericRepository<User>
{
    /// <summary>
    /// Gets a user by email address.
    /// </summary>
    /// <param name="email">The email address.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The user or null if not found.</returns>
    Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Checks if a user exists with the specified email.
    /// </summary>
    /// <param name="email">The email address.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if exists, false otherwise.</returns>
    Task<bool> ExistsByEmailAsync(string email, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets users by role.
    /// </summary>
    /// <param name="role">The user role.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Collection of users with the specified role.</returns>
    Task<IEnumerable<User>> GetByRoleAsync(string role, CancellationToken cancellationToken = default);
}