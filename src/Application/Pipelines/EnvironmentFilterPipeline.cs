using EnvironmentsService.Src.Application.DTOs.GetRequest;
using EnvironmentsService.Src.Application.Strategies.Interfaces;

namespace EnvironmentsService.Src.Application.Pipelines;

public class EnvironmentFilterPipeline(IEnumerable<IEnvironmentFilterStrategy> strategies)
{
    private readonly IEnumerable<IEnvironmentFilterStrategy> _strategies = strategies;

    public IQueryable<Domain.Entities.Environment> ApplyFilters(
        IQueryable<Domain.Entities.Environment> query,
        GetAvailableEnvironmentsRequest request)
    {
        foreach (var strategy in _strategies)
        {
            query = strategy.Apply(query, request);
        }

        return query;
    }
}
