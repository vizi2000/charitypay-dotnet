using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using CharityPay.Application.Organizations.Queries.GetOrganizations;
using CharityPay.Application.Organizations.Queries.GetOrganizationById;
using CharityPay.Application.Organizations.Queries.GetOrganizationStats;
using CharityPay.Application.Organizations.Commands.GenerateQrCode;
using CharityPay.API.Controllers.Base;

namespace CharityPay.API.Controllers.v1;

[ApiVersion(1.0)]
[Route("api/v{version:apiVersion}/[controller]")]
public class OrganizationsController : BaseController
{
    public OrganizationsController(ISender sender) : base(sender) { }

    /// <summary>
    /// Get all approved organizations
    /// </summary>
    /// <param name="query">Query parameters for filtering and pagination</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of organizations</returns>
    [HttpGet]
    public async Task<IActionResult> GetOrganizations([FromQuery] GetOrganizationsQuery query, CancellationToken cancellationToken)
    {
        var result = await Sender.Send(query, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Error);
    }

    /// <summary>
    /// Get organization by ID
    /// </summary>
    /// <param name="id">Organization ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Organization details</returns>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetOrganizationById(Guid id, CancellationToken cancellationToken)
    {
        var query = new GetOrganizationByIdQuery(id);
        var result = await Sender.Send(query, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : NotFound(result.Error);
    }

    /// <summary>
    /// Get organization statistics
    /// </summary>
    /// <param name="id">Organization ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Organization statistics</returns>
    [HttpGet("{id:guid}/stats")]
    public async Task<IActionResult> GetOrganizationStats(Guid id, CancellationToken cancellationToken)
    {
        var query = new GetOrganizationStatsQuery(id);
        var result = await Sender.Send(query, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : NotFound(result.Error);
    }

    /// <summary>
    /// Generate QR code for organization
    /// </summary>
    /// <param name="id">Organization ID</param>
    /// <param name="size">QR code size (default: 200)</param>
    /// <param name="format">Image format (default: png)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>QR code image</returns>
    [HttpGet("{id:guid}/qr")]
    public async Task<IActionResult> GenerateQrCode(Guid id, [FromQuery] int size = 200, [FromQuery] string format = "png", CancellationToken cancellationToken)
    {
        var command = new GenerateQrCodeCommand(id, size, format);
        var result = await Sender.Send(command, cancellationToken);
        
        if (!result.IsSuccess)
            return NotFound(result.Error);

        var contentType = format.ToLower() switch
        {
            "png" => "image/png",
            "jpeg" => "image/jpeg",
            "jpg" => "image/jpeg",
            _ => "image/png"
        };

        return File(result.Value, contentType);
    }

    /// <summary>
    /// Download QR code for organization (high quality)
    /// </summary>
    /// <param name="id">Organization ID</param>
    /// <param name="size">QR code size (default: 400)</param>
    /// <param name="format">Image format (default: png)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>QR code image for download</returns>
    [HttpGet("{id:guid}/qr/download")]
    public async Task<IActionResult> DownloadQrCode(Guid id, [FromQuery] int size = 400, [FromQuery] string format = "png", CancellationToken cancellationToken)
    {
        var command = new GenerateQrCodeCommand(id, size, format);
        var result = await Sender.Send(command, cancellationToken);
        
        if (!result.IsSuccess)
            return NotFound(result.Error);

        var contentType = format.ToLower() switch
        {
            "png" => "image/png",
            "jpeg" => "image/jpeg",
            "jpg" => "image/jpeg",
            _ => "image/png"
        };

        var fileName = $"qr-code-{id}.{format}";
        return File(result.Value, contentType, fileName);
    }
}