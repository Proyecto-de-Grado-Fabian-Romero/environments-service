using AutoMapper;
using EnvironmentsService.Src.Application.DTOs.Responses;
using EnvironmentsService.Src.Application.Interfaces;
using EnvironmentsService.Src.Application.Ports;
using EnvironmentsService.Src.Domain.Events;
using EnvironmentsService.Src.Domain.Interfaces;

namespace EnvironmentsService.Src.Application.Commands.Concretes;

public class UpdateReservationStatusCommand(
    Guid reservationPublicId,
    string newStatus,
    IReservationRepository resRepo,
    IAdminServiceAdapter adminServiceAdapter,
    IMapper mapper,
    INotificationsPublisher notificationsPublisher
)
{
    private readonly Guid _reservationPublicId = reservationPublicId;
    private readonly string _newStatus = newStatus.ToLower();
    private readonly IReservationRepository _resRepo = resRepo;
    private readonly IAdminServiceAdapter _adminServiceAdapter = adminServiceAdapter;
    private readonly INotificationsPublisher _notificationsPublisher = notificationsPublisher;
    private readonly IMapper _mapper = mapper;

    public async Task<ReservationResponse> ExecuteAsync()
    {
        var reservation =
            await _resRepo.GetByPublicIdAsync(_reservationPublicId)
            ?? throw new Exception("La reserva no existe.");

        if (reservation.Status == "rejected")
        {
            throw new Exception("No se puede cambiar el estado de una reserva rechazada.");
        }

        var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        var latestEnd = reservation.TimeRanges.Max(r => r.EndDate);
        if (latestEnd < now)
        {
            throw new Exception("No se puede cambiar el estado de una reserva ya pasada.");
        }

        if (_newStatus == "paid")
        {
            bool overlaps = await _resRepo.ExistsOverlappingConfirmedAsync(
                reservation.EnvironmentId,
                reservation.Id,
                reservation.TimeRanges
            );

            if (overlaps)
            {
                throw new Exception("Ya existe una reserva confirmada para este periodo.");
            }

            decimal precioTotal = reservation.TotalPrice;
            decimal descuento = precioTotal * 0.10m;
            decimal precioConDescuento = precioTotal - descuento;

            Console.WriteLine($"OwnerId: {reservation.OwnerId}");
            Console.WriteLine($"ReservationId: {reservation.PublicId}");

            await _adminServiceAdapter.RequestOwnerIncomeAsync(
                reservation.OwnerId,
                reservation.PublicId,
                precioConDescuento,
                reservation.Currency,
                DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
            );

            var requestNotification = new SendToUserMessage
            {
                UserPublicId = reservation.OwnerId,
                Title = "Nueva Reserva Pagada",
                Message = "Tienes una nueva reserva pagada de uno de tus ambientes",
                Type = "Info",
                Channel = "Push",
            };

            _notificationsPublisher.PublishSendToUser(requestNotification);
            var boliviaOffset = TimeSpan.FromHours(-4);
            var nowBolivia = DateTimeOffset.UtcNow.ToOffset(boliviaOffset);
            reservation.ConfirmedAt = nowBolivia.ToUnixTimeSeconds();
        }
        else if (newStatus == "confirmed")
        {
            var requestNotification = new SendToUserMessage
            {
                UserPublicId = reservation.RenterId,
                Title = "Nueva Reserva Confirmada",
                Message = "Tienes una nueva reserva confirmada de una de tus solicitudes. Ya puedes realizar el pago.",
                Type = "Info",
                Channel = "Push",
            };

            _notificationsPublisher.PublishSendToUser(requestNotification);
            var boliviaOffset = TimeSpan.FromHours(-4);
            var nowBolivia = DateTimeOffset.UtcNow.ToOffset(boliviaOffset);
            reservation.ConfirmedAt = nowBolivia.ToUnixTimeSeconds();
        }
        else if (newStatus == "rejected")
        {
            var requestNotification = new SendToUserMessage
            {
                UserPublicId = reservation.RenterId,
                Title = "Reserva Rechazada",
                Message = "Tienes una nueva reserva rechazada de una de tus solicitudes",
                Type = "Info",
                Channel = "Push",
            };
        }

        reservation.Status = _newStatus;
        await _resRepo.SaveChangesAsync();

        return _mapper.Map<ReservationResponse>(reservation);
    }
}
