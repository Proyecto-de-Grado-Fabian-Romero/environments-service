using EnvironmentsService.Src.Application.DTOs.Create;
using EnvironmentsService.Src.Application.DTOs.Get;
using EnvironmentsService.Src.Application.DTOs.Responses;

namespace EnvironmentsService.Src.Application.Interfaces;

public interface IReservationService
{
    Task<ReservationResponse> CreateAsync(CreateReservationRequest request, Guid renterId);

    Task<PagedResult<ReservationResponse>> GetByUserAsync(Guid userId, string? status, int page, int limit);

    Task<ReservationResponse?> GetByPublicIdAsync(Guid publicId);

    Task<ReservationResponse> UpdateStatusAsync(Guid reservationPublicId, string newStatus);

    Task<List<ReservationResponse>> GetConflictingReservationsAsync(Guid environmentId, long start, long end);

    Task<List<ReservationResponse>> GetByOwnerAndDayAsync(Guid ownerId, long timestamp);
}
