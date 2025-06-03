namespace EnvironmentsService.Src.Application.Mapping;

using AutoMapper;
using EnvironmentsService.Src.Application.DTOs.Create;
using EnvironmentsService.Src.Application.DTOs.Get;
using EnvironmentsService.Src.Application.DTOs.Responses;
using EnvironmentsService.Src.Domain.Entities;
using EnvironmentsService.Src.Domain.Entities.Booking;

public class EnvironmentProfile : Profile
{
    public EnvironmentProfile()
    {
        CreateMap<Environment, EnvironmentDto>()
            .ForMember(dest => dest.Services, opt => opt.MapFrom(src => src.EnvironmentServices.Select(es => es.Service)))
            .ForMember(dest => dest.EnvironmentAreas, opt => opt.MapFrom(src => src.EnvironmentAreas))
            .ForMember(dest => dest.PricingPolicies, opt => opt.MapFrom(src => src.PricingPolicies))
            .ForMember(dest => dest.WeeklySchedules, opt => opt.MapFrom(src => src.WeeklySchedules))
            .ForMember(dest => dest.SpecialAvailabilities, opt => opt.MapFrom(src => src.SpecialAvailabilities))
            .ForMember(dest => dest.DiscountPolicies, opt => opt.MapFrom(src => src.DiscountPolicies))
            .ForMember(dest => dest.Equipment, opt => opt.MapFrom(src => src.Equipment))
            .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.Type))
            .ForMember(dest => dest.PhotoUrls, opt => opt.MapFrom(src => src.Photos.Select(p => p.Url)))
            .ReverseMap();

        CreateMap<Environment, GetAllEnvironmentDto>()
            .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.Type))
            .ForMember(dest => dest.PricingPolicies, opt => opt.MapFrom(src => src.PricingPolicies))
            .ForMember(dest => dest.PhotoUrls, opt => opt.MapFrom(src => src.Photos.Select(p => p.Url)))
            .ReverseMap();

        CreateMap<CreateEnvironmentDto, Environment>()
           .ForMember(dest => dest.Latitude, opt => opt.MapFrom(src => src.Latitude))
           .ForMember(dest => dest.Longitude, opt => opt.MapFrom(src => src.Longitude))
           .ForMember(dest => dest.Location, opt => opt.MapFrom(src => src.Location))
           .ForMember(dest => dest.TypeId, opt => opt.Ignore())
           .ForMember(dest => dest.EnvironmentServices, opt => opt.Ignore())
           .ForMember(dest => dest.EnvironmentAreas, opt => opt.Ignore())
           .ForMember(dest => dest.Photos, opt => opt.Ignore())
           .ForMember(dest => dest.Equipment, opt => opt.Ignore())
           .ForMember(dest => dest.PricingPolicies, opt => opt.MapFrom(src => src.PricingPolicies))
           .ForMember(dest => dest.DiscountPolicies, opt => opt.MapFrom(src => src.DiscountPolicies))
           .ForMember(dest => dest.WeeklySchedules, opt => opt.MapFrom(src => src.WeeklySchedules));

        CreateMap<Area, AreaDto>().ReverseMap();
        CreateMap<EnvironmentPhoto, EnvironmentPhotoDto>().ReverseMap();
        CreateMap<EnvironmentType, EnvironmentTypeDto>().ReverseMap();
        CreateMap<NonAvailability, NonAvailabilityDto>().ReverseMap();
        CreateMap<PricingPolicy, PricingPolicyDto>().ReverseMap();
        CreateMap<DiscountPolicy, DiscountPolicyDto>().ReverseMap();
        CreateMap<Service, ServiceDto>().ReverseMap();
        CreateMap<WeeklySchedule, WeeklyScheduleDto>().ReverseMap();
        CreateMap<SpecialAvailability, SpecialAvailabilityDto>().ReverseMap();
        CreateMap<EnvironmentArea, EnvironmentAreaDto>().ReverseMap();

        CreateMap<ReservationTimeRange, TimeRangeDto>().ReverseMap();
        CreateMap<Reservation, ReservationResponse>()
            .ForMember(dest => dest.EnvironmentTitle, opt => opt.MapFrom(src => src.Environment.Title))
            .ForMember(dest => dest.OwnerId, opt => opt.MapFrom(src => src.Environment.OwnerId))
            .ForMember(dest => dest.RentalUnit, opt => opt.MapFrom(src => src.Environment.RentalUnit))
            .ForMember(dest => dest.EnvironmentPhotoUrl, opt => opt.MapFrom(src =>
                src.Environment.Photos.OrderBy(p => p.Order).Select(p => p.Url).FirstOrDefault()))
            .ForMember(dest => dest.TimeRanges, opt => opt.MapFrom(src => src.TimeRanges))
            .ForMember(dest => dest.TotalPrice, opt => opt.MapFrom(src => src.TotalPrice))
            .ForMember(dest => dest.Currency, opt => opt.MapFrom(src => src.Currency))
            .ForMember(dest => dest.Payments, opt => opt.MapFrom(src => src.Payments));

        CreateMap<ReservationPayment, ReservationPaymentResponse>();

        CreateMap<CreateReservationRequest, CreateReservationDto>()
            .ForMember(dest => dest.RenterId, opt => opt.Ignore());
    }
}
