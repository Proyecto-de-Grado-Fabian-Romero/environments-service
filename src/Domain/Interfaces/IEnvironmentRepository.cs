using EnvironmentsService.Src.Application.DTOs.GetRequest;

namespace EnvironmentsService.Src.Domain.Interfaces;

public interface IEnvironmentRepository
{
    Task<(List<Src.Domain.Entities.Environment> Environments, int TotalItems)> FilterEnvironmentsAsync(
    GetAvailableEnvironmentsRequest request, int page, int limit);

    Task<Src.Domain.Entities.Environment?> GetSingleEnvironment(Guid publicId);
}
