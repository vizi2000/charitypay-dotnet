using Microsoft.Extensions.DependencyInjection;
using FluentValidation;
using CharityPay.Application.Abstractions.Services;
using CharityPay.Application.Services;

namespace CharityPay.Application.Extensions;

/// <summary>
/// Extension methods for configuring application services.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds application layer services to the dependency injection container.
    /// </summary>
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        // Add AutoMapper
        services.AddAutoMapper(typeof(ServiceCollectionExtensions).Assembly);
        
        // Add FluentValidation
        services.AddValidatorsFromAssembly(typeof(ServiceCollectionExtensions).Assembly);
        
        // Add application services
        services.AddScoped<IAuthenticationService, AuthenticationService>();
        services.AddScoped<IOrganizationService, OrganizationService>();
        services.AddScoped<IPaymentService, PaymentService>();
        services.AddScoped<IMerchantOnboardingService, MerchantOnboardingService>();
        services.AddScoped<IRefreshTokenService, RefreshTokenService>();
        
        // Add MediatR if using CQRS pattern
        // services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(ServiceCollectionExtensions).Assembly));
        
        return services;
    }
}