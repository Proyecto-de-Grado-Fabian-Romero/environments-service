using System.Text.Json;
using EnvironmentsService.Src.Application.DTOs.GetRequest;
using EnvironmentsService.Src.Application.Strategies.Interfaces;

namespace EnvironmentsService.Src.Application.Strategies.Concretes.GetEnvironments;

public class FilterByEquipmentStrategy : IEnvironmentPostFilterStrategy
{
    public IEnumerable<Domain.Entities.Environment> Apply(
        IEnumerable<Domain.Entities.Environment> environments,
        GetAvailableEnvironmentsRequest request)
    {
        if (request.EquipmentRequired == null || !request.EquipmentRequired.Any())
        {
            return environments;
        }

        return environments.Where(env =>
        {
            try
            {
                var eq = JsonSerializer.Deserialize<Dictionary<string, int>>(env.Equipment);
                if (eq == null)
                {
                    return false;
                }

                return request.EquipmentRequired.All(req =>
                    eq.TryGetValue(req.Key, out var qty) && qty >= req.Value);
            }
            catch
            {
                return false;
            }
        });
    }
}
