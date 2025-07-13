using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using CharityPay.Application.Abstractions;
using CharityPay.Application.Abstractions.Repositories;
using CharityPay.Infrastructure.Data;
using CharityPay.Infrastructure.Data.Repositories;

namespace CharityPay.Infrastructure.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Add Unit of Work
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        
        // Add Repositories
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IOrganizationRepository, OrganizationRepository>();
        services.AddScoped<IPaymentRepository, PaymentRepository>();
        
        return services;
    }
}