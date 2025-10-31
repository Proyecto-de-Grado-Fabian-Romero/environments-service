using AutoMapper;
using EnvironmentsService.Src.Application.Commands.Interfaces;
using EnvironmentsService.Src.Application.DTOs.Get;
using EnvironmentsService.Src.Application.DTOs.GetRequest;
using EnvironmentsService.Src.Domain.Interfaces;

namespace EnvironmentsService.Src.Application.Commands.Concretes;

public class GetAllEnvironmentsCommand(
    IEnvironmentRepository repository,
    IMapper mapper,
    GetAvailableEnvironmentsRequest request,
    int page,
    int limit,
    Guid? userPublicId
) : ICommand<PagedResult<GetAllEnvironmentDto>>
{
    private readonly IEnvironmentRepository _repository = repository;
    private readonly IMapper _mapper = mapper;
    private readonly GetAvailableEnvironmentsRequest _request = request;
    private readonly int _page = page;
    private readonly int _limit = limit;
    private readonly Guid? _userPublicId = userPublicId;

    public async Task<PagedResult<GetAllEnvironmentDto>> ExecuteAsync()
    {
        var (environments, totalItems) = await _repository.FilterEnvironmentsAsync(
            _request,
            _page,
            _limit,
            _userPublicId
        );
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
