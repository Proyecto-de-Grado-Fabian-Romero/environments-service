using EnvironmentsService.Src.Application.DTOs.Create;
using EnvironmentsService.Src.Application.DTOs.Responses;

namespace EnvironmentsService.Src.Application.Interfaces;

public interface IReservationService
{
    Task<ReservationResponse> CreateAsync(CreateReservationRequest request, Guid renterId);

    Task<List<ReservationResponse>> GetByUserAsync(Guid userId, string? status, int page, int limit);
}
