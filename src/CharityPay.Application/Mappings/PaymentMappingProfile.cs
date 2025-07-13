using AutoMapper;
using CharityPay.Application.DTOs.Payment;
using CharityPay.Application.DTOs.Organization;
using CharityPay.Domain.Entities;

namespace CharityPay.Application.Mappings;

/// <summary>
/// AutoMapper profile for Payment entity mappings.
/// </summary>
public class PaymentMappingProfile : Profile
{
    public PaymentMappingProfile()
    {
        CreateMap<Payment, PaymentDto>()
            .ForMember(dest => dest.OrganizationName, opt => opt.MapFrom(src => src.Organization != null ? src.Organization.Name : string.Empty));
            
        CreateMap<Payment, RecentDonationDto>()
            .ForMember(dest => dest.DonatedAt, opt => opt.MapFrom(src => src.CreatedAt));
    }
}