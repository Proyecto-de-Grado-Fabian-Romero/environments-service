using AutoMapper;
using EnvironmentsService.Src.Application.Commands.Interfaces;
using EnvironmentsService.Src.Application.DTOs.Get;
using EnvironmentsService.Src.Domain.Interfaces;

namespace EnvironmentsService.Src.Application.Commands.Concretes;

public class GetSingleEnvironmentCommand(
    IEnvironmentRepository repository,
    IMapper mapper,
    Guid publicId
) : ICommand<EnvironmentDto?>
{
    private readonly IEnvironmentRepository _repository = repository;
    private readonly IMapper _mapper = mapper;
    private readonly Guid _publicId = publicId;

    public async Task<EnvironmentDto?> ExecuteAsync()
    {
        var environment = await _repository.GetSingleEnvironment(_publicId);
        return _mapper.Map<EnvironmentDto>(environment);
    }
}
