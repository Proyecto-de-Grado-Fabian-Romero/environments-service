using AutoMapper;
using EnvironmentsService.Src.Application.Commands.Concretes;
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
        var command = new GetAllEnvironmentsCommand(_repository, _mapper, request, page, limit);
        return await command.ExecuteAsync();
    }

    public async Task<EnvironmentDto?> GetSingleEnvironmentAsync(Guid publicId)
    {
        var command = new GetSingleEnvironmentCommand(_repository, _mapper, publicId);
        return await command.ExecuteAsync();
    }
}
