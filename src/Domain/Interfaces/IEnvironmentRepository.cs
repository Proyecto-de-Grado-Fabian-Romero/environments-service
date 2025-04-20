using EnvironmentsService.Src.Application.DTOs.GetRequest;

namespace EnvironmentsService.Src.Domain.Interfaces;

public interface IEnvironmentRepository
{
    Task<(List<Entities.Environment> Environments, int TotalItems)> FilterEnvironmentsAsync(
    GetAvailableEnvironmentsRequest request, int page, int limit);

    Task<Entities.Environment?> GetSingleEnvironment(Guid publicId);

    Task AddAsync(Entities.Environment environment);

    Task SaveChangesAsync();
}
