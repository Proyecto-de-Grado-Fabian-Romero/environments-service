namespace EnvironmentsService.Src.Application.Services;

using AutoMapper;
using EnvironmentsService.Src.Application.Commands.Concretes;
using EnvironmentsService.Src.Application.DTOs.Create;
using EnvironmentsService.Src.Application.DTOs.Responses;
using EnvironmentsService.Src.Application.Interfaces;
using EnvironmentsService.Src.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

public class ReservationService(
    IEnvironmentRepository environmentRepo,
    IReservationRepository reservationRepo,
    IMapper mapper) : IReservationService
{
    private readonly IEnvironmentRepository _envRepo = environmentRepo;
    private readonly IReservationRepository _resRepo = reservationRepo;
    private readonly IMapper _mapper = mapper;

    public async Task<ReservationResponse> CreateAsync(CreateReservationRequest request, Guid renterId)
    {
        var dto = _mapper.Map<CreateReservationDto>(request);
        dto.RenterId = renterId;
        var command = new CreateReservationCommand(dto, _envRepo, _resRepo, _mapper);
        return await command.ExecuteAsync();
    }

    public async Task<List<ReservationResponse>> GetByUserAsync(Guid userId, string? status, int page, int limit)
    {
        var validStatuses = new[] { "pending", "confirmed", "rejected", "cancelled", "paid" };
        var normalizedStatus = validStatuses.Contains(status?.ToLower()) ? status!.ToLower() : "confirmed";

        var reservations = await _resRepo.GetUserReservationsAsync(userId, normalizedStatus, page, limit);
        return _mapper.Map<List<ReservationResponse>>(reservations);
    }

    public async Task<ReservationResponse?> GetByPublicIdAsync(Guid publicId)
    {
        var reservation = await _resRepo.GetByPublicIdAsync(publicId);
        return reservation == null ? null : _mapper.Map<ReservationResponse>(reservation);
    }

    public async Task<ReservationResponse> UpdateStatusAsync(Guid reservationPublicId, string newStatus)
    {
        var command = new UpdateReservationStatusCommand(reservationPublicId, newStatus, _resRepo, _mapper);
        return await command.ExecuteAsync();
    }
}
