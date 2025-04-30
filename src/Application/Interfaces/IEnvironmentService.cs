using EnvironmentsService.Src.Application.DTOs.Create;
using EnvironmentsService.Src.Application.DTOs.Get;
using EnvironmentsService.Src.Application.DTOs.GetRequest;

namespace EnvironmentsService.Src.Application.Interfaces;

public interface IEnvironmentService
{
    Task<PagedResult<GetAllEnvironmentDto>> GetAvailableEnvironmentsAsync(GetAvailableEnvironmentsRequest request, int page, int limit);

    Task<PagedResult<GetAllEnvironmentDto>> GetOwnerEnvironmentsAsync(Guid publicUserId, int page, int limit);

    Task<EnvironmentDto?> GetSingleEnvironmentAsync(Guid publicId);

    Task<EnvironmentDto> CreateAsync(CreateEnvironmentDto dto, Guid userId);
}
