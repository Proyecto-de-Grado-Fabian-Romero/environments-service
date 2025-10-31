using EnvironmentsService.Src.Application.DTOs.GetRequest;

namespace EnvironmentsService.Src.Application.Strategies.Interfaces;

public interface IEnvironmentFilterStrategy
{
    IQueryable<Domain.Entities.Environment> Apply(
            IQueryable<Domain.Entities.Environment> query,
            GetAvailableEnvironmentsRequest request);
}
