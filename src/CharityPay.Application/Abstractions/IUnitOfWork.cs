using CharityPay.Application.Abstractions.Repositories;

namespace CharityPay.Application.Abstractions;

/// <summary>
/// Unit of Work pattern interface for managing transactions across multiple repositories.
/// </summary>
public interface IUnitOfWork : IDisposable
{
    /// <summary>
    /// Gets the user repository.
    /// </summary>
    IUserRepository Users { get; }
    
    /// <summary>
    /// Gets the organization repository.
    /// </summary>
    IOrganizationRepository Organizations { get; }
    
    /// <summary>
    /// Gets the payment repository.
    /// </summary>
    IPaymentRepository Payments { get; }
    
    /// <summary>
    /// Saves all changes made in this unit of work to the database.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The number of state entries written to the database.</returns>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}