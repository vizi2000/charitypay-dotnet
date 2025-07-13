using AutoMapper;
using CharityPay.Application.DTOs.Organization;
using CharityPay.Domain.Entities;

namespace CharityPay.Application.Mappings;

/// <summary>
/// AutoMapper profile for Organization entity mappings.
/// </summary>
public class OrganizationMappingProfile : Profile
{
    public OrganizationMappingProfile()
    {
        CreateMap<Organization, OrganizationDto>();
        
        CreateMap<UpdateOrganizationRequest, Organization>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.Name, opt => opt.Ignore())
            .ForMember(dest => dest.Category, opt => opt.Ignore())
            .ForMember(dest => dest.Location, opt => opt.Ignore())
            .ForMember(dest => dest.TargetAmount, opt => opt.Ignore())
            .ForMember(dest => dest.CollectedAmount, opt => opt.Ignore())
            .ForMember(dest => dest.Status, opt => opt.Ignore())
            .ForMember(dest => dest.UserId, opt => opt.Ignore())
            .ForMember(dest => dest.User, opt => opt.Ignore())
            .ForMember(dest => dest.Payments, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore());
    }
}