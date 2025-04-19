using EnvironmentsService.Src.Application.DTOs.Get;
using EnvironmentsService.Src.Application.DTOs.GetRequest;

namespace EnvironmentsService.Src.Application.Interfaces;

public interface IEnvironmentService
{
    Task<PagedResult<GetAllEnvironmentDto>> GetAvailableEnvironmentsAsync(GetAvailableEnvironmentsRequest request, int page, int limit);

    Task<EnvironmentDto?> GetSingleEnvironment(Guid publicId);
}
