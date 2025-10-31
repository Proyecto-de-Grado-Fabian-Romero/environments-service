using AutoMapper;
using EnvironmentsService.Src.Application.Commands.Interfaces;
using EnvironmentsService.Src.Application.DTOs.Get;
using EnvironmentsService.Src.Domain.Interfaces;

namespace EnvironmentsService.Src.Application.Commands.Concretes;

public class GetOwnerEnvironments(
    IEnvironmentRepository repository,
    IMapper mapper,
    Guid publicUserId,
    int page,
    int limit) : ICommand<PagedResult<GetAllEnvironmentDto>>
{
    private readonly IEnvironmentRepository _repository = repository;
    private readonly IMapper _mapper = mapper;
    private readonly Guid _userId = publicUserId;
    private readonly int _page = page;
    private readonly int _limit = limit;

    public async Task<PagedResult<GetAllEnvironmentDto>> ExecuteAsync()
    {
        var (environments, totalItems) = await _repository.GetOwnerEnvironmentsAsync(_userId, _page, _limit);
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
