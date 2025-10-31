using EnvironmentsService.Src.Application.DTOs.GetRequest;
using EnvironmentsService.Src.Domain.Entities;

namespace EnvironmentsService.Src.Domain.Interfaces;

public interface IEnvironmentRepository
{
    Task<(List<Entities.Environment> Environments, int TotalItems)> FilterEnvironmentsAsync(
        GetAvailableEnvironmentsRequest request,
        int page,
        int limit,
        Guid? userPublicId
    );

    Task<(List<Entities.Environment> Environments, int TotalItems)> GetOwnerEnvironmentsAsync(
        Guid pubUserId,
        int page,
        int limit
    );

    Task<Entities.Environment?> GetSingleEnvironment(Guid publicId);

    Task AddAsync(Entities.Environment environment);

    Task SaveChangesAsync();

    Task AddImageAsync(EnvironmentPhoto image);

    Task UpdateDetectedEquipmentAsync(Guid publicId, string serializedEquipment);

    Task<List<Entities.Environment>> GetFilteredEnvironmentsAsync(
        GetAvailableEnvironmentsRequest request
    );

    Task<Domain.Entities.Environment?> GetByPublicIdWithIncludesAsync(Guid publicId);

    Task RemovePhotoAsync(EnvironmentPhoto photo);

    Task<EnvironmentPhoto?> GetPhotoByFileIdAsync(string fileId);

    Task<bool> SetHiddenByPublicIdAsync(Guid environmentPublicId, bool hide, Guid userPublicId);

    Task<bool> SoftDeleteByPublicIdAsync(Guid environmentPublicId, Guid userPublicId);
}
