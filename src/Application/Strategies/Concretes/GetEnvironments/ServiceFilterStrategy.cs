using EnvironmentsService.Src.Application.DTOs.GetRequest;
using EnvironmentsService.Src.Application.Strategies.Interfaces;

namespace EnvironmentsService.Src.Application.Strategies.Concretes.GetEnvironments;

public class ServiceFilterStrategy : IEnvironmentFilterStrategy
{
    public IQueryable<Domain.Entities.Environment> Apply(
            IQueryable<Domain.Entities.Environment> query,
            GetAvailableEnvironmentsRequest request)
    {
        if (request.ServicePublicKeys?.Count > 0)
        {
            query = query.Where(e =>
                request.ServicePublicKeys.All(reqKey =>
                    e.EnvironmentServices.Any(es => es.Service.PublicKey == reqKey)));
        }

        return query;
    }
}
