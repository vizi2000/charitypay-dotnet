using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Text.Json;
using CharityPay.Domain.Shared;
using CharityPay.Application.Exceptions;
using Serilog;

namespace CharityPay.API.Middleware;

public class GlobalExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionHandlingMiddleware> _logger;

    public GlobalExceptionHandlingMiddleware(RequestDelegate next, ILogger<GlobalExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unhandled exception occurred while processing the request");
            await HandleExceptionAsync(context, ex);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var response = context.Response;
        response.ContentType = "application/json";

        var (statusCode, error) = GetErrorResponse(exception);
        response.StatusCode = (int)statusCode;

        var result = JsonSerializer.Serialize(new ProblemDetails
        {
            Title = error.Code,
            Detail = error.Message,
            Status = (int)statusCode,
            Instance = context.Request.Path
        });

        await response.WriteAsync(result);
    }

    private static (HttpStatusCode statusCode, Error error) GetErrorResponse(Exception exception)
    {
        return exception switch
        {
            ValidationException validationEx => (
                HttpStatusCode.BadRequest,
                Error.Create("ValidationError", validationEx.Message)
            ),
            
            NotFoundException notFoundEx => (
                HttpStatusCode.NotFound,
                Error.Create("NotFound", notFoundEx.Message)
            ),
            
            UnauthorizedException unauthorizedEx => (
                HttpStatusCode.Unauthorized,
                Error.Create("Unauthorized", unauthorizedEx.Message)
            ),
            
            ForbiddenException forbiddenEx => (
                HttpStatusCode.Forbidden,
                Error.Create("Forbidden", forbiddenEx.Message)
            ),
            
            ConflictException conflictEx => (
                HttpStatusCode.Conflict,
                Error.Create("Conflict", conflictEx.Message)
            ),
            
            ExternalServiceException externalEx => (
                HttpStatusCode.BadGateway,
                Error.Create("ExternalServiceError", externalEx.Message)
            ),
            
            ArgumentException argumentEx => (
                HttpStatusCode.BadRequest,
                Error.Create("InvalidArgument", argumentEx.Message)
            ),
            
            ArgumentNullException argumentNullEx => (
                HttpStatusCode.BadRequest,
                Error.Create("InvalidArgument", argumentNullEx.Message)
            ),
            
            InvalidOperationException invalidOpEx => (
                HttpStatusCode.BadRequest,
                Error.Create("InvalidOperation", invalidOpEx.Message)
            ),
            
            _ => (
                HttpStatusCode.InternalServerError,
                Error.Create("InternalServerError", "An error occurred while processing your request")
            )
        };
    }
}