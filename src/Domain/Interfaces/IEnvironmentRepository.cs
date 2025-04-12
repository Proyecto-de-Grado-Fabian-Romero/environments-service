using EnvironmentsService.Src.Application.DTOs.GetRequest;

namespace EnvironmentsService.src.Domain.Interfaces;

public interface IEnvironmentRepository
{
    Task<List<Src.Domain.Entities.Environment>> FilterEnvironmentsAsync(GetAvailableEnvironmentsRequest request);
}
