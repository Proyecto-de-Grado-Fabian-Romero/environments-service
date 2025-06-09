using EnvironmentsService.Src.Application.DTOs.GetRequest;
using EnvironmentsService.Src.Application.Strategies.Interfaces;

namespace EnvironmentsService.Src.Application.Pipelines;

public class EnvironmentPostFilterPipeline(IEnumerable<IEnvironmentPostFilterStrategy> strategies)
{
    public IEnumerable<Domain.Entities.Environment> ApplyFilters(IEnumerable<Domain.Entities.Environment> environments, GetAvailableEnvironmentsRequest request)
    {
        foreach (var strategy in strategies)
        {
            environments = strategy.Apply(environments, request);
        }

        return environments;
    }
}
