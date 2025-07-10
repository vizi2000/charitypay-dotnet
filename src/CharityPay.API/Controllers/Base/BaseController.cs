using Microsoft.AspNetCore.Mvc;
using MediatR;
using CharityPay.Domain.Shared;
using System.Security.Claims;

namespace CharityPay.API.Controllers.Base;

[ApiController]
public abstract class BaseController : ControllerBase
{
    protected readonly ISender Sender;

    protected BaseController(ISender sender)
    {
        Sender = sender;
    }

    protected Guid? GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(userIdClaim, out var userId) ? userId : null;
    }

    protected string? GetCurrentUserEmail()
    {
        return User.FindFirst(ClaimTypes.Email)?.Value;
    }

    protected string? GetCurrentUserRole()
    {
        return User.FindFirst(ClaimTypes.Role)?.Value;
    }

    protected bool IsAdmin()
    {
        return GetCurrentUserRole() == "admin";
    }

    protected bool IsOrganization()
    {
        return GetCurrentUserRole() == "organization";
    }

    protected IActionResult HandleResult<T>(Result<T> result)
    {
        if (result.IsSuccess)
            return Ok(result.Value);

        return result.Error?.Code switch
        {
            "NotFound" => NotFound(result.Error),
            "ValidationError" => BadRequest(result.Error),
            "Unauthorized" => Unauthorized(result.Error),
            "Forbidden" => Forbid(result.Error.Message),
            "Conflict" => Conflict(result.Error),
            _ => BadRequest(result.Error)
        };
    }

    protected IActionResult HandleResult(Result result)
    {
        if (result.IsSuccess)
            return Ok();

        return result.Error?.Code switch
        {
            "NotFound" => NotFound(result.Error),
            "ValidationError" => BadRequest(result.Error),
            "Unauthorized" => Unauthorized(result.Error),
            "Forbidden" => Forbid(result.Error.Message),
            "Conflict" => Conflict(result.Error),
            _ => BadRequest(result.Error)
        };
    }

    protected string GetClientIpAddress()
    {
        return HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
    }

    protected string GetUserAgent()
    {
        return Request.Headers["User-Agent"].ToString();
    }

    protected string GetCorrelationId()
    {
        return Request.Headers["X-Correlation-ID"].FirstOrDefault() ?? Guid.NewGuid().ToString();
    }
}