using EnvironmentsService.Src.Application.DTOs.GetRequest;
using EnvironmentsService.Src.Application.Strategies.Interfaces;

namespace EnvironmentsService.Src.Application.Strategies.Concretes.GetEnvironments;

public class EnvironmentTypeFilterStrategy : IEnvironmentFilterStrategy
{
    public IQueryable<Domain.Entities.Environment> Apply(
           IQueryable<Domain.Entities.Environment> query,
           GetAvailableEnvironmentsRequest request)
    {
        if (!string.IsNullOrWhiteSpace(request.EnvironmentTypePublicKey))
        {
            query = query.Where(e => e.Type.PublicKey == request.EnvironmentTypePublicKey);
        }

        return query;
    }
}
