using AutoMapper;
using EnvironmentsService.Src.Application.DTOs.Get;
using EnvironmentsService.Src.Application.Interfaces;
using EnvironmentsService.Src.Domain.Interfaces;

namespace EnvironmentsService.Src.Application.Services;

public class ServiceService(IServiceRepository serviceRepository, IMapper mapper) : IServiceService
{
    private readonly IServiceRepository _serviceRepository = serviceRepository;
    private readonly IMapper _mapper = mapper;

    public async Task<List<ServiceDto>> GetAllAsync()
    {
        var services = await _serviceRepository.GetAllAsync();
        return _mapper.Map<List<ServiceDto>>(services);
    }
}
