using EnvironmentsService.src.Application.DTOs.Create;
using EnvironmentsService.Src.Application.DTOs.Get;
using EnvironmentsService.Src.Application.DTOs.Responses;

namespace EnvironmentsService.Src.Application.Interfaces;

public interface IAvailabilityService
{
    Task<List<TimeRange>> GetUnavailableTimeSlotsAsync(Guid envId, long start, long end);

    Task BlockDateAsync(BlockAvailabilityRequest request, Guid ownerId);

    Task<List<OwnerBlockedAvailabilityDto>> GetOwnerBlockedAsync(Guid ownerId);
}
