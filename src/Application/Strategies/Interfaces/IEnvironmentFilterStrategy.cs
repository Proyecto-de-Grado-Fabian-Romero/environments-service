using EnvironmentsService.Src.Application.DTOs.GetRequest;

namespace EnvironmentsService.Src.Application.Strategies.Interfaces;

public interface IEnvironmentFilterStrategy
{
    IQueryable<Src.Domain.Entities.Environment> Apply(
            IQueryable<Src.Domain.Entities.Environment> query,
            GetAvailableEnvironmentsRequest request);
}
