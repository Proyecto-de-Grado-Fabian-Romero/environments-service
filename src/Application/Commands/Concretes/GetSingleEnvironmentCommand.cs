using AutoMapper;
using EnvironmentsService.Src.Application.Commands.Interfaces;
using EnvironmentsService.Src.Application.DTOs.Get;
using EnvironmentsService.src.Domain.Interfaces;

namespace EnvironmentsService.Src.Application.Commands.Concretes;

public class GetSingleEnvironmentCommand : ICommand<EnvironmentDto?>
{
    private readonly IEnvironmentRepository _repository;
    private readonly IMapper _mapper;
    private readonly Guid _publicId;

    public GetSingleEnvironmentCommand(
        IEnvironmentRepository repository,
        IMapper mapper,
        Guid publicId)
    {
        _repository = repository;
        _mapper = mapper;
        _publicId = publicId;
    }

    public async Task<EnvironmentDto?> ExecuteAsync()
    {
        var environment = await _repository.GetSingleEnvironment(_publicId);
        return _mapper.Map<EnvironmentDto>(environment);
    }
}
