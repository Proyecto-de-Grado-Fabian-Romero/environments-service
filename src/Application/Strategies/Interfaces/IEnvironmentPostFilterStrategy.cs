using EnvironmentsService.Src.Application.DTOs.GetRequest;

namespace EnvironmentsService.Src.Application.Strategies.Interfaces;

public interface IEnvironmentPostFilterStrategy
{
    IEnumerable<Domain.Entities.Environment> Apply(IEnumerable<Domain.Entities.Environment> environments, GetAvailableEnvironmentsRequest request);
}
