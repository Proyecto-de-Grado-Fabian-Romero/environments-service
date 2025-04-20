using AutoMapper;
using EnvironmentsService.Src.Application.DTOs.Get;
using EnvironmentsService.src.Application.Interfaces;
using EnvironmentsService.src.Domain.Interfaces;

namespace EnvironmentsService.src.Application.Services;

public class AreaService(IAreaRepository areaRepository, IMapper mapper) : IAreaService
{
    private readonly IAreaRepository _areaRepository = areaRepository;
    private readonly IMapper _mapper = mapper;

    public async Task<List<AreaDto>> GetAllAsync()
    {
        var areas = await _areaRepository.GetAllAsync();
        return _mapper.Map<List<AreaDto>>(areas);
    }
}