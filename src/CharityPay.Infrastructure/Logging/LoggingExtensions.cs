using Microsoft.Extensions.Logging;
using System.Runtime.CompilerServices;

namespace CharityPay.Infrastructure.Logging;

public static class LoggingExtensions
{
    public static void LogMethodEntry(this ILogger logger, string message = "", [CallerMemberName] string methodName = "")
    {
        logger.LogDebug("Entering {MethodName} {Message}", methodName, message);
    }

    public static void LogMethodExit(this ILogger logger, string message = "", [CallerMemberName] string methodName = "")
    {
        logger.LogDebug("Exiting {MethodName} {Message}", methodName, message);
    }

    public static void LogBusinessError(this ILogger logger, string message, Exception? exception = null)
    {
        logger.LogError(exception, "Business Error: {Message}", message);
    }

    public static void LogValidationError(this ILogger logger, string message, object? validationErrors = null)
    {
        logger.LogWarning("Validation Error: {Message} {ValidationErrors}", message, validationErrors);
    }

    public static void LogPaymentEvent(this ILogger logger, string eventType, Guid paymentId, decimal amount, string status)
    {
        logger.LogInformation("Payment Event: {EventType} - PaymentId: {PaymentId}, Amount: {Amount}, Status: {Status}", 
            eventType, paymentId, amount, status);
    }

    public static void LogOrganizationEvent(this ILogger logger, string eventType, Guid organizationId, string organizationName)
    {
        logger.LogInformation("Organization Event: {EventType} - OrganizationId: {OrganizationId}, Name: {OrganizationName}", 
            eventType, organizationId, organizationName);
    }

    public static void LogUserEvent(this ILogger logger, string eventType, Guid userId, string userEmail)
    {
        logger.LogInformation("User Event: {EventType} - UserId: {UserId}, Email: {UserEmail}", 
            eventType, userId, userEmail);
    }

    public static void LogExternalApiCall(this ILogger logger, string apiName, string endpoint, string method, TimeSpan duration)
    {
        logger.LogInformation("External API Call: {ApiName} {Method} {Endpoint} - Duration: {Duration}ms", 
            apiName, method, endpoint, duration.TotalMilliseconds);
    }

    public static void LogExternalApiError(this ILogger logger, string apiName, string endpoint, string method, Exception exception)
    {
        logger.LogError(exception, "External API Error: {ApiName} {Method} {Endpoint}", 
            apiName, method, endpoint);
    }

    public static void LogDatabaseOperation(this ILogger logger, string operation, string entityType, Guid? entityId = null)
    {
        logger.LogDebug("Database Operation: {Operation} on {EntityType} {EntityId}", 
            operation, entityType, entityId);
    }

    public static void LogSecurityEvent(this ILogger logger, string eventType, string? userEmail = null, string? ipAddress = null)
    {
        logger.LogWarning("Security Event: {EventType} - User: {UserEmail}, IP: {IpAddress}", 
            eventType, userEmail, ipAddress);
    }

    public static void LogPerformanceMetric(this ILogger logger, string operation, TimeSpan duration, object? metadata = null)
    {
        logger.LogInformation("Performance Metric: {Operation} - Duration: {Duration}ms {Metadata}", 
            operation, duration.TotalMilliseconds, metadata);
    }
}