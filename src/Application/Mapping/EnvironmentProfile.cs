namespace EnvironmentsService.Src.Application.Mapping;

using AutoMapper;
using EnvironmentsService.Src.Application.DTOs.Get;
using EnvironmentsService.Src.Domain.Entities;

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
            .ForMember(dest => dest.LastTour360Date, opt => opt.MapFrom(src =>
                src.Tour360Requests
                    .Where(r => r.ScheduledDate.HasValue)
                    .OrderByDescending(r => r.ScheduledDate)
                    .FirstOrDefault()))
            .ReverseMap();

        CreateMap<Environment, GetAllEnvironmentDto>()
            .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.Type))
            .ForMember(dest => dest.PricingPolicies, opt => opt.MapFrom(src => src.PricingPolicies))
            .ForMember(dest => dest.PhotoUrls, opt => opt.MapFrom(src => src.Photos.Select(p => p.Url)))
            .ForMember(dest => dest.LastTour360Date, opt => opt.MapFrom(src =>
                src.Tour360Requests
                    .Where(r => r.ScheduledDate.HasValue)
                    .OrderByDescending(r => r.ScheduledDate)
                    .FirstOrDefault()))
            .ReverseMap();

        CreateMap<Area, AreaDto>().ReverseMap();
        CreateMap<EnvironmentPhoto, EnvironmentPhotoDto>().ReverseMap();
        CreateMap<EnvironmentType, EnvironmentTypeDto>().ReverseMap();
        CreateMap<NonAvailability, NonAvailabilityDto>().ReverseMap();
        CreateMap<PricingPolicy, PricingPolicyDto>().ReverseMap();
        CreateMap<DiscountPolicy, DiscountPolicyDto>().ReverseMap();
        CreateMap<Service, ServiceDto>().ReverseMap();
        CreateMap<Tour360Request, Tour360RequestDto>().ReverseMap();
        CreateMap<WeeklySchedule, WeeklyScheduleDto>().ReverseMap();
        CreateMap<SpecialAvailability, SpecialAvailabilityDto>().ReverseMap();
        CreateMap<EnvironmentArea, EnvironmentAreaDto>().ReverseMap();
    }
}
