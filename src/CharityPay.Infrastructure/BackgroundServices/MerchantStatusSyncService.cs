using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using CharityPay.Application.Abstractions;
using CharityPay.Application.Abstractions.Services;
using CharityPay.Application.Services;
using CharityPay.Domain.Enums;

namespace CharityPay.Infrastructure.BackgroundServices;

/// <summary>
/// Background service that periodically checks merchant status with Polcard
/// as a fallback for missed webhooks
/// </summary>
public class MerchantStatusSyncService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<MerchantStatusSyncService> _logger;
    private readonly TimeSpan _syncInterval = TimeSpan.FromMinutes(30);

    public MerchantStatusSyncService(
        IServiceProvider serviceProvider,
        ILogger<MerchantStatusSyncService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Merchant Status Sync Service started");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await SyncMerchantStatusesAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred during merchant status synchronization");
            }

            await Task.Delay(_syncInterval, stoppingToken);
        }

        _logger.LogInformation("Merchant Status Sync Service stopped");
    }

    private async Task SyncMerchantStatusesAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
        var polcardClient = scope.ServiceProvider.GetRequiredService<IPolcardCoPilotClient>();
        var merchantOnboardingService = scope.ServiceProvider.GetRequiredService<IMerchantOnboardingService>();

        try
        {
            // Get organizations that are in intermediate states and have Polcard merchant IDs
            var organizationsToCheck = await unitOfWork.Organizations.GetByStatusAsync(
                OrganizationStatus.KycSubmitted, cancellationToken);

            var pendingOrganizations = organizationsToCheck
                .Where(o => !string.IsNullOrEmpty(o.PolcardMerchantId))
                .ToList();

            if (!pendingOrganizations.Any())
            {
                _logger.LogDebug("No organizations require status synchronization");
                return;
            }

            _logger.LogInformation("Synchronizing status for {Count} organizations", pendingOrganizations.Count);

            foreach (var organization in pendingOrganizations)
            {
                if (cancellationToken.IsCancellationRequested)
                    break;

                try
                {
                    await SyncOrganizationStatusAsync(organization.PolcardMerchantId!, 
                        merchantOnboardingService, polcardClient, cancellationToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to sync status for organization {OrganizationId} with merchant ID {MerchantId}",
                        organization.Id, organization.PolcardMerchantId);
                }

                // Small delay between requests to avoid rate limiting
                await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken);
            }

            _logger.LogInformation("Completed merchant status synchronization");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during merchant status synchronization batch");
            throw;
        }
    }

    private async Task SyncOrganizationStatusAsync(
        string merchantId,
        IMerchantOnboardingService merchantOnboardingService,
        IPolcardCoPilotClient polcardClient,
        CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogDebug("Checking status for merchant {MerchantId}", merchantId);

            var statusResponse = await polcardClient.GetMerchantStatusAsync(merchantId, cancellationToken);

            // Process the status update if it's different from current state
            await merchantOnboardingService.ProcessMerchantStatusUpdateAsync(
                merchantId, statusResponse.Status, statusResponse.Reason, cancellationToken);

            _logger.LogDebug("Successfully synced status for merchant {MerchantId}: {Status}",
                merchantId, statusResponse.Status);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogWarning(ex, "HTTP error while checking status for merchant {MerchantId}", merchantId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while syncing status for merchant {MerchantId}", merchantId);
            throw;
        }
    }
}