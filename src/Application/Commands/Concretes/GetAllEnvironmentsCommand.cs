using AutoMapper;
using EnvironmentsService.Src.Application.Commands.Interfaces;
using EnvironmentsService.Src.Application.DTOs.Get;
using EnvironmentsService.Src.Application.DTOs.GetRequest;
using EnvironmentsService.src.Domain.Interfaces;

namespace EnvironmentsService.Src.Application.Commands.Concretes;

public class GetAllEnvironmentsCommand : ICommand<PagedResult<GetAllEnvironmentDto>>
{
    private readonly IEnvironmentRepository _repository;
    private readonly IMapper _mapper;
    private readonly GetAvailableEnvironmentsRequest _request;
    private readonly int _page;
    private readonly int _limit;

    public GetAllEnvironmentsCommand(
        IEnvironmentRepository repository,
        IMapper mapper,
        GetAvailableEnvironmentsRequest request,
        int page,
        int limit)
    {
        _repository = repository;
        _mapper = mapper;
        _request = request;
        _page = page;
        _limit = limit;
    }

    public async Task<PagedResult<GetAllEnvironmentDto>> ExecuteAsync()
    {
        var (environments, totalItems) = await _repository.FilterEnvironmentsAsync(_request, _page, _limit);
        var dtos = _mapper.Map<List<GetAllEnvironmentDto>>(environments);

        return new PagedResult<GetAllEnvironmentDto>
        {
            Items = dtos,
            CurrentPage = _page,
            TotalPages = (int)Math.Ceiling(totalItems / (double)_limit),
            Limit = _limit,
            TotalItems = totalItems,
        };
    }
}
