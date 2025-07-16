using AutoMapper;
using Microsoft.Extensions.Logging;
using CharityPay.Application.Abstractions;
using CharityPay.Application.Abstractions.Services;
using CharityPay.Application.Common.Models;
using CharityPay.Application.DTOs.Organization;
using CharityPay.Application.DTOs.Payment;
using CharityPay.Domain.Enums;

namespace CharityPay.Application.Services;

/// <summary>
/// Implementation of organization management service.
/// </summary>
public class OrganizationService : IOrganizationService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<OrganizationService> _logger;
    
    public OrganizationService(
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ILogger<OrganizationService> logger)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
    }
    
    public async Task<PaginatedResponse<OrganizationDto>> GetOrganizationsAsync(
        int page = 1, 
        int pageSize = 10, 
        string? category = null, 
        string? location = null,
        CancellationToken cancellationToken = default)
    {
        var (organizations, totalCount) = await _unitOfWork.Organizations
            .GetApprovedPaginatedAsync(page, pageSize, cancellationToken);
        
        var organizationDtos = _mapper.Map<IEnumerable<OrganizationDto>>(organizations);
        
        return new PaginatedResponse<OrganizationDto>
        {
            Items = organizationDtos,
            CurrentPage = page,
            PageSize = pageSize,
            TotalCount = totalCount
        };
    }
    
    public async Task<OrganizationDto?> GetOrganizationByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var organization = await _unitOfWork.Organizations.GetByIdAsync(id, cancellationToken);
        
        if (organization == null || organization.Status != OrganizationStatus.Active)
        {
            return null;
        }
        
        return _mapper.Map<OrganizationDto>(organization);
    }
    
    public async Task<OrganizationDto?> GetMyOrganizationAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var organization = await _unitOfWork.Organizations.GetByUserIdAsync(userId, cancellationToken);
        
        if (organization == null)
        {
            return null;
        }
        
        return _mapper.Map<OrganizationDto>(organization);
    }
    
    public async Task<OrganizationDto> UpdateOrganizationAsync(
        Guid userId, 
        UpdateOrganizationRequest request, 
        CancellationToken cancellationToken = default)
    {
        var organization = await _unitOfWork.Organizations.GetByUserIdAsync(userId, cancellationToken);
        
        if (organization == null)
        {
            throw new InvalidOperationException("Organization not found");
        }
        
        organization.UpdateProfile(
            request.Website,
            request.Phone,
            request.Address,
            request.PrimaryColor,
            request.SecondaryColor,
            request.CustomMessage
        );
        
        _unitOfWork.Organizations.Update(organization);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        
        _logger.LogInformation("Organization {OrganizationId} updated by user {UserId}", 
            organization.Id, userId);
        
        return _mapper.Map<OrganizationDto>(organization);
    }
    
    public async Task<string> UpdateOrganizationLogoAsync(
        Guid userId, 
        Stream logoStream, 
        string fileName,
        CancellationToken cancellationToken = default)
    {
        var organization = await _unitOfWork.Organizations.GetByUserIdAsync(userId, cancellationToken);
        
        if (organization == null)
        {
            throw new InvalidOperationException("Organization not found");
        }
        
        // TODO: Implement file storage service
        var logoUrl = $"/uploads/logos/{organization.Id}/{fileName}";
        
        organization.UpdateLogo(logoUrl);
        
        _unitOfWork.Organizations.Update(organization);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        
        _logger.LogInformation("Organization {OrganizationId} logo updated", organization.Id);
        
        return logoUrl;
    }
    
    public async Task<OrganizationStatsDto> GetOrganizationStatsAsync(
        Guid organizationId, 
        CancellationToken cancellationToken = default)
    {
        var organization = await _unitOfWork.Organizations.GetByIdAsync(organizationId, cancellationToken);
        
        if (organization == null)
        {
            throw new InvalidOperationException("Organization not found");
        }
        
        var payments = await _unitOfWork.Payments.GetByOrganizationIdAsync(organizationId, cancellationToken);
        var completedPayments = payments.Where(p => p.Status == PaymentStatus.Completed).ToList();
        
        var todayStart = DateTime.UtcNow.Date;
        var monthStart = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1);
        
        var stats = new OrganizationStatsDto
        {
            TotalCollected = completedPayments.Sum(p => p.Amount),
            TotalDonations = completedPayments.Count,
            AverageDonation = completedPayments.Any() ? completedPayments.Average(p => p.Amount) : 0,
            TodayAmount = completedPayments.Where(p => p.CreatedAt >= todayStart).Sum(p => p.Amount),
            MonthAmount = completedPayments.Where(p => p.CreatedAt >= monthStart).Sum(p => p.Amount),
            ProgressPercentage = organization.TargetAmount > 0 
                ? Math.Round((organization.CollectedAmount / organization.TargetAmount) * 100, 2) 
                : 0,
            RecentDonations = completedPayments
                .OrderByDescending(p => p.CreatedAt)
                .Take(10)
                .Select(p => new RecentDonationDto
                {
                    Amount = p.Amount,
                    DonorName = p.DonorName,
                    DonatedAt = p.CreatedAt
                })
                .ToList()
        };
        
        return stats;
    }
    
    public async Task<IEnumerable<OrganizationDto>> SearchOrganizationsAsync(
        string searchTerm, 
        CancellationToken cancellationToken = default)
    {
        var organizations = await _unitOfWork.Organizations.SearchByNameAsync(searchTerm, cancellationToken);
        
        return _mapper.Map<IEnumerable<OrganizationDto>>(organizations);
    }
}