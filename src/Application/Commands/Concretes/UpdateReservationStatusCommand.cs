using AutoMapper;
using EnvironmentsService.Src.Application.DTOs.Responses;
using EnvironmentsService.Src.Application.Interfaces;
using EnvironmentsService.Src.Domain.Interfaces;

namespace EnvironmentsService.Src.Application.Commands.Concretes;

public class UpdateReservationStatusCommand(
    Guid reservationPublicId,
    Guid ownerId,
    string newStatus,
    IReservationRepository resRepo,
    IAdminServiceAdapter adminServiceAdapter,
    IMapper mapper)
{
    private readonly Guid _reservationPublicId = reservationPublicId;
    private readonly string _newStatus = newStatus.ToLower();
    private readonly IReservationRepository _resRepo = resRepo;
    private readonly IAdminServiceAdapter _adminServiceAdapter = adminServiceAdapter;
    private readonly IMapper _mapper = mapper;

    public async Task<ReservationResponse> ExecuteAsync()
    {
        var reservation = await _resRepo.GetByPublicIdAsync(_reservationPublicId)
            ?? throw new Exception("La reserva no existe.");

        if (reservation.Status == "rejected")
        {
            throw new Exception("No se puede cambiar el estado de una reserva rechazada.");
        }

        var now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        var latestEnd = reservation.TimeRanges.Max(r => r.EndDate);
        if (latestEnd < now)
        {
            throw new Exception("No se puede cambiar el estado de una reserva ya pasada.");
        }

        if (_newStatus == "confirmed")
        {
            bool overlaps = await _resRepo.ExistsOverlappingConfirmedAsync(
                reservation.EnvironmentId,
                reservation.Id,
                reservation.TimeRanges);

            if (overlaps)
            {
                throw new Exception("Ya existe una reserva confirmada para este periodo.");
            }

            await _adminServiceAdapter.RequestOwnerIncomeAsync(
            ownerId,
            reservation.Id,
            reservation.TotalPrice,
            reservation.Currency,
            DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());
        }

        reservation.Status = _newStatus;
        await _resRepo.SaveChangesAsync();
        return _mapper.Map<ReservationResponse>(reservation);
    }
}
