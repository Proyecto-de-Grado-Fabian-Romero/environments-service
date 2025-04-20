using EnvironmentsService.Src.Application.DTOs.GetRequest;
using EnvironmentsService.Src.Application.Strategies.Interfaces;

namespace EnvironmentsService.Src.Application.Strategies.Concretes.GetEnvironments;

public class LocationFilterStrategy : IEnvironmentFilterStrategy
{
    public IQueryable<Domain.Entities.Environment> Apply(
            IQueryable<Domain.Entities.Environment> query,
            GetAvailableEnvironmentsRequest request)
    {
        if (!string.IsNullOrWhiteSpace(request.Location))
        {
            query = query.Where(e => e.Location.Contains(request.Location));
        }

        return query;
    }
}
