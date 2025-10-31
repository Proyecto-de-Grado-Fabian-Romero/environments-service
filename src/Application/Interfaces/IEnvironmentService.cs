using EnvironmentsService.Src.Application.DTOs.Create;
using EnvironmentsService.Src.Application.DTOs.Get;
using EnvironmentsService.Src.Application.DTOs.GetRequest;
using EnvironmentsService.Src.Application.DTOs.Update;

namespace EnvironmentsService.Src.Application.Interfaces;

public interface IEnvironmentService
{
    Task<PagedResult<GetAllEnvironmentDto>> GetAvailableEnvironmentsAsync(
        GetAvailableEnvironmentsRequest request,
        int page,
        int limit,
        Guid? userPublicId
    );

    Task<PagedResult<GetAllEnvironmentDto>> GetOwnerEnvironmentsAsync(
        Guid publicUserId,
        int page,
        int limit
    );

    Task<EnvironmentDto?> GetSingleEnvironmentAsync(Guid publicId);

    Task<EnvironmentDto> CreateAsync(CreateEnvironmentDto dto, Guid userId);

    Task UpdateDetectedObjectsAsync(Guid publicId, Dictionary<string, int> detectedObjects);

    Task<List<AvailableEquipmentDto>> GetAvailableEquipmentAsync(
        GetAvailableEnvironmentsRequest request
    );

    Task<EnvironmentDto> UpdateAsync(Guid publicId, UpdateEnvironmentDto dto, Guid userId);

    Task<bool> PatchHideEnvironmentAsync(Guid environmentPublicId, bool hide, Guid userPublicId);

    Task<bool> PatchDeleteEnvironmentAsync(Guid environmentPublicId, Guid userPublicId);
}
