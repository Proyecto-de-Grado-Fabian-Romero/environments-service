using EnvironmentsService.Src.Application.DTOs.GetRequest;
using EnvironmentsService.Src.Application.Strategies.Interfaces;

namespace EnvironmentsService.Src.Application.Strategies.Concretes.GetEnvironments;

public class DateAvailabilityFilterStrategy : IEnvironmentFilterStrategy
{
    public IQueryable<Domain.Entities.Environment> Apply(
        IQueryable<Domain.Entities.Environment> query,
        GetAvailableEnvironmentsRequest request)
    {
        if (request.StartDate.HasValue && request.EndDate.HasValue)
        {
            query = query.Where(e => !e.NonAvailabilities.Any(n =>
                n.StartDate <= request.StartDate && n.EndDate >= request.EndDate));
        }

        return query;
    }
}
