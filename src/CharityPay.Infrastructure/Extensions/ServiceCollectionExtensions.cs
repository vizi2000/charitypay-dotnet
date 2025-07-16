using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using CharityPay.Application.Abstractions;
using CharityPay.Application.Abstractions.Repositories;
using CharityPay.Application.Abstractions.Services;
using CharityPay.Infrastructure.Data;
using CharityPay.Infrastructure.Data.Repositories;
using CharityPay.Infrastructure.ExternalServices.Polcard;
using CharityPay.Infrastructure.ExternalServices.Polcard.Configuration;
using CharityPay.Infrastructure.BackgroundServices;

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
        
        // Configure Polcard settings
        services.Configure<PolcardSettings>(configuration.GetSection(PolcardSettings.SectionName));
        
        // Add HTTP client for Polcard
        services.AddHttpClient<IPolcardCoPilotClient, PolcardCoPilotClient>();
        
        // Add Polcard client
        services.AddScoped<IPolcardCoPilotClient, PolcardCoPilotClient>();
        
        // Add background services
        services.AddHostedService<MerchantStatusSyncService>();
        
        return services;
    }
}