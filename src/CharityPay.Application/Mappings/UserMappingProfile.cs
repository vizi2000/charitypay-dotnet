using AutoMapper;
using CharityPay.Application.DTOs.Auth;
using CharityPay.Domain.Entities;

namespace CharityPay.Application.Mappings;

/// <summary>
/// AutoMapper profile for User entity mappings.
/// </summary>
public class UserMappingProfile : Profile
{
    public UserMappingProfile()
    {
        CreateMap<User, UserDto>()
            .ForMember(dest => dest.OrganizationId, opt => opt.MapFrom(src => src.Organization != null ? src.Organization.Id : (Guid?)null))
            .ForMember(dest => dest.OrganizationName, opt => opt.MapFrom(src => src.Organization != null ? src.Organization.Name : null));
    }
}