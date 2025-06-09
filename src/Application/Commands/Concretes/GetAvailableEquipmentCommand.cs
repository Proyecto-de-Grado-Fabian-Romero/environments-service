using System.Text.Json;
using EnvironmentsService.Src.Application.Commands.Interfaces;
using EnvironmentsService.Src.Application.DTOs.Get;
using EnvironmentsService.Src.Application.DTOs.GetRequest;
using EnvironmentsService.Src.Domain.Interfaces;

namespace EnvironmentsService.Src.Application.Commands.Concretes;

public class GetAvailableEquipmentCommand(
    GetAvailableEnvironmentsRequest request,
    IEnvironmentRepository repository)
    : ICommand<List<AvailableEquipmentDto>>
{
    private readonly IEnvironmentRepository _repository = repository;
    private readonly GetAvailableEnvironmentsRequest _request = request;

    public async Task<List<AvailableEquipmentDto>> ExecuteAsync()
    {
        var environments = await _repository.GetFilteredEnvironmentsAsync(_request);

        var equipmentCounts = new Dictionary<string, int>();

        foreach (var env in environments)
        {
            if (string.IsNullOrWhiteSpace(env.Equipment))
            {
                continue;
            }

            Dictionary<string, int>? equipmentDict = null;

            try
            {
                equipmentDict = JsonSerializer.Deserialize<Dictionary<string, int>>(env.Equipment);
            }
            catch
            {
                continue;
            }

            if (equipmentDict == null)
            {
                continue;
            }

            foreach (var key in equipmentDict.Keys.Distinct())
            {
                if (equipmentDict[key] <= 0)
                {
                    continue;
                }

                if (equipmentCounts.ContainsKey(key))
                {
                    equipmentCounts[key]++;
                }
                else
                {
                    equipmentCounts[key] = 1;
                }
            }
        }

        return [.. equipmentCounts
            .Select(kv => new AvailableEquipmentDto
            {
                Name = kv.Key,
                Count = kv.Value,
            })
            .OrderByDescending(e => e.Count)];
    }
}
