using EnvironmentsService.Src.Application.DTOs.GetRequest;
using EnvironmentsService.Src.Application.Strategies.Interfaces;

namespace EnvironmentsService.Src.Application.Pipelines;

public class EnvironmentFilterPipeline
{
    private readonly IEnumerable<IEnvironmentFilterStrategy> _strategies;

    public EnvironmentFilterPipeline(IEnumerable<IEnvironmentFilterStrategy> strategies)
    {
        _strategies = strategies;
    }

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
