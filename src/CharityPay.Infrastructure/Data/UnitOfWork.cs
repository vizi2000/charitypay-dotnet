using CharityPay.Application.Abstractions.Repositories;
using CharityPay.Infrastructure.Data.Repositories;

namespace CharityPay.Infrastructure.Data;

public class UnitOfWork : IUnitOfWork
{
    private readonly CharityPayDbContext _context;
    private IUserRepository? _userRepository;
    private IOrganizationRepository? _organizationRepository;
    private IPaymentRepository? _paymentRepository;

    public UnitOfWork(CharityPayDbContext context)
    {
        _context = context;
    }

    public IUserRepository UserRepository => _userRepository ??= new UserRepository(_context);
    public IOrganizationRepository OrganizationRepository => _organizationRepository ??= new OrganizationRepository(_context);
    public IPaymentRepository PaymentRepository => _paymentRepository ??= new PaymentRepository(_context);

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }

    public void Dispose()
    {
        _context.Dispose();
    }

    public async ValueTask DisposeAsync()
    {
        await _context.DisposeAsync();
    }
}