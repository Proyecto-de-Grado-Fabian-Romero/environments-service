using AutoMapper;
using EnvironmentsService.Src.Application.DTOs.Get;
using EnvironmentsService.Src.Application.DTOs.GetRequest;
using EnvironmentsService.Src.Application.Interfaces;
using EnvironmentsService.src.Domain.Interfaces;

namespace EnvironmentsService.Src.Application.Services;

public class EnvironmentService(IEnvironmentRepository repository, IMapper mapper) : IEnvironmentService
{
    private readonly IEnvironmentRepository _repository = repository;
    private readonly IMapper _mapper = mapper;

    public async Task<PagedResult<GetAllEnvironmentDto>> GetAvailableEnvironmentsAsync(GetAvailableEnvironmentsRequest request, int page, int limit)
    {
        var (environments, totalItems) = await _repository.FilterEnvironmentsAsync(request, page, limit);

        int totalPages = (int)Math.Ceiling(totalItems / (double)limit);

        var environmentDtos = _mapper.Map<List<GetAllEnvironmentDto>>(environments);

        return new PagedResult<GetAllEnvironmentDto>
        {
            Items = environmentDtos,
            CurrentPage = page,
            TotalPages = totalPages,
            Limit = limit,
            TotalItems = totalItems,
        };
    }

    public async Task<EnvironmentDto?> GetSingleEnvironment(Guid publicId)
    {
        var environment = await _repository.GetSingleEnvironment(publicId);
        var environmentDto = _mapper.Map<EnvironmentDto>(environment);

        return environmentDto;
    }
}
