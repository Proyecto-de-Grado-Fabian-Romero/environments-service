using EnvironmentsService.Src.Application.DTOs.GetRequest;
using EnvironmentsService.Src.Application.Strategies.Interfaces;

namespace EnvironmentsService.Src.Application.Strategies.Concretes.GetEnvironments;

public class AreaFilterStrategy : IEnvironmentFilterStrategy
{
    public IQueryable<Domain.Entities.Environment> Apply(
        IQueryable<Domain.Entities.Environment> query,
        GetAvailableEnvironmentsRequest request
    )
    {
        if (request?.Areas == null || !request.Areas.Any())
        {
            return query;
        }

        foreach (var areaReq in request.Areas)
        {
            var areaPublicKey = (areaReq.AreaPublicKey ?? string.Empty).Trim();
            var minQuantity = areaReq.MinQuantity;

            if (string.IsNullOrEmpty(areaPublicKey))
            {
                continue;
            }

            query = query.Where(e =>
                e.EnvironmentAreas.Any(ea =>
                    ea.Area != null
                    && ea.Area.PublicKey != null
                    && ea.Area.PublicKey.Trim().ToLower() == areaPublicKey.ToLower()
                    && ea.Quantity >= minQuantity
                )
            );
        }

        return query;
    }
}
