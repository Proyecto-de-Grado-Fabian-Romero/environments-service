using System.Text.Json;
using EnvironmentsService.Src.Application.Commands.Interfaces;
using EnvironmentsService.Src.Domain.Interfaces;

namespace EnvironmentsService.Src.Application.Commands.Concretes;

public class UpdateDetectedObjectsCommand(
    IEnvironmentRepository repository,
    Guid publicId,
    Dictionary<string, int> equipment) : ICommand<string>
{
    private readonly IEnvironmentRepository _repository = repository;
    private readonly Guid _publicId = publicId;
    private readonly Dictionary<string, int> _equipment = equipment;

    public async Task<string> ExecuteAsync()
    {
        var serialized = JsonSerializer.Serialize(_equipment);

        await _repository.UpdateDetectedEquipmentAsync(_publicId, serialized);
        return serialized;
    }
}
