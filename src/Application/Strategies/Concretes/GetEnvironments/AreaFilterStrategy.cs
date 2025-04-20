using EnvironmentsService.Src.Application.DTOs.GetRequest;
using EnvironmentsService.Src.Application.Strategies.Interfaces;

namespace EnvironmentsService.Src.Application.Strategies.Concretes.GetEnvironments;

public class AreaFilterStrategy : IEnvironmentFilterStrategy
{
    public IQueryable<Domain.Entities.Environment> Apply(
            IQueryable<Domain.Entities.Environment> query,
            GetAvailableEnvironmentsRequest request)
    {
        if (request.Areas?.Count > 0)
        {
            foreach (var (areaPublicKey, minQuantity) in request.Areas)
            {
                query = query.Where(e =>
                    e.EnvironmentAreas.Any(ea =>
                        ea.Area.PublicKey == areaPublicKey && ea.Quantity >= minQuantity));
            }
        }

        return query;
    }
}
