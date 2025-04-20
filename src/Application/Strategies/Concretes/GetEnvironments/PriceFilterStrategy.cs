using EnvironmentsService.Src.Application.DTOs.GetRequest;
using EnvironmentsService.Src.Application.Strategies.Interfaces;

namespace EnvironmentsService.Src.Application.Strategies.Concretes.GetEnvironments;

public class PriceFilterStrategy : IEnvironmentFilterStrategy
{
    public IQueryable<Domain.Entities.Environment> Apply(
            IQueryable<Domain.Entities.Environment> query,
            GetAvailableEnvironmentsRequest request)
    {
        if (request.MinPrice.HasValue || request.MaxPrice.HasValue)
        {
            query = query.Where(e =>
                e.PricingPolicies.Any(p =>
                    (!request.MinPrice.HasValue || p.BasePrice >= request.MinPrice) &&
                    (!request.MaxPrice.HasValue || p.BasePrice <= request.MaxPrice)));
        }

        return query;
    }
}
