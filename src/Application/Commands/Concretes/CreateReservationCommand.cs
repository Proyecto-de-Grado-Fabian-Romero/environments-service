namespace EnvironmentsService.Src.Application.Commands.Concretes;

using AutoMapper;
using EnvironmentsService.Src.Application.Commands.Interfaces;
using EnvironmentsService.Src.Application.DTOs.Create;
using EnvironmentsService.Src.Application.DTOs.Responses;
using EnvironmentsService.Src.Application.Ports;
using EnvironmentsService.Src.Domain.Entities.Booking;
using EnvironmentsService.Src.Domain.Events;
using EnvironmentsService.Src.Domain.Interfaces;

public class CreateReservationCommand(
    CreateReservationDto request,
    IEnvironmentRepository environmentRepo,
    IReservationRepository reservationRepo,
    IMapper mapper,
    INotificationsPublisher notificationsPublisher
) : ICommand<ReservationResponse>
{
    private readonly IEnvironmentRepository _environmentRepo = environmentRepo;
    private readonly IReservationRepository _reservationRepo = reservationRepo;
    private readonly IMapper _mapper = mapper;
    private readonly INotificationsPublisher _notificationsPublisher = notificationsPublisher;

    public async Task<ReservationResponse> ExecuteAsync()
    {
        using var transaction = await _reservationRepo.BeginTransactionAsync();

        var environment =
            await _environmentRepo.GetSingleEnvironment(request.EnvironmentId)
            ?? throw new Exception("El entorno no existe.");

        foreach (var range in request.TimeRanges)
        {
            // Convertimos UNIX seconds -> DateTime
            var startDateTime = DateTimeOffset.FromUnixTimeSeconds(range.StartDate).DateTime;
            var endDateTime = DateTimeOffset.FromUnixTimeSeconds(range.EndDate).DateTime;

            // Recorremos todos los días que abarca la reserva
            for (var date = startDateTime.Date; date <= endDateTime.Date; date = date.AddDays(1))
            {
                // Para comparar horarios, convertimos TODO a minutos
                var resStartMinutes = (int)(
                    date == startDateTime.Date
                        ? startDateTime.TimeOfDay.TotalMinutes
                        : 0 // Inicio del día
                );

                var resEndMinutes = (int)(
                    date == endDateTime.Date
                        ? endDateTime.TimeOfDay.TotalMinutes
                        : 1440 // Fin del día: 24*60
                );

                // === VALIDACIÓN SPECIAL AVAILABILITY ===
                var special = environment.SpecialAvailabilities
                    .FirstOrDefault(sa => sa.Date.Date == date.Date);

                if (special != null)
                {
                    if (!special.IsAvailable)
                    {
                        throw new Exception($"El entorno no está disponible el {date:yyyy-MM-dd}.");
                    }

                    // special.StartTime y EndTime vienen en milisegundos -> convertir a minutos
                    var specialStartMinutes = (int)TimeSpan.FromMilliseconds(special.StartTime).TotalMinutes;
                    var specialEndMinutes = (int)TimeSpan.FromMilliseconds(special.EndTime).TotalMinutes;

                    if (resStartMinutes < specialStartMinutes || resEndMinutes > specialEndMinutes)
                    {
                        throw new Exception(
                            $"La reserva en {date:yyyy-MM-dd} debe estar entre " +
                            $"{TimeSpan.FromMinutes(specialStartMinutes):hh\\:mm} y " +
                            $"{TimeSpan.FromMinutes(specialEndMinutes):hh\\:mm}."
                        );
                    }
                }
                // === VALIDACIÓN WEEKLY SCHEDULE ===
                else if (environment.RentalUnit == "Horas")
                {
                    var dayOfWeek = (int)date.DayOfWeek;

                    var weekly = environment.WeeklySchedules
                        .FirstOrDefault(ws => ws.DayOfWeek == dayOfWeek)
                        ?? throw new Exception($"El entorno no está disponible el día {date:dddd}.");

                    if (resStartMinutes < weekly.StartTime || resEndMinutes > weekly.EndTime)
                    {
                        throw new Exception(
                            $"La reserva en {date:yyyy-MM-dd} debe estar entre " +
                            $"{TimeSpan.FromMinutes(weekly.StartTime):hh\\:mm} y " +
                            $"{TimeSpan.FromMinutes(weekly.EndTime):hh\\:mm}."
                        );
                    }
                }
            }
        }

        var existingReservations = await _reservationRepo.GetActiveReservationsByRenterAsync(
            request.RenterId
        );

        var overlappingReservationIds = new HashSet<Guid>();

        var newRanges = request
            .TimeRanges.Select(nr => new
            {
                Start = DateTimeOffset.FromUnixTimeSeconds(nr.StartDate).UtcDateTime,
                End = DateTimeOffset.FromUnixTimeSeconds(nr.EndDate).UtcDateTime,
            })
            .ToList();

        foreach (var existing in existingReservations)
        {
            if (existing.TimeRanges == null || !existing.TimeRanges.Any())
            {
                continue;
            }

            bool reservationOverlaps = false;

            // foreach (var existingRange in existing.TimeRanges)
            // {
            //     var exStart = DateTimeOffset
            //         .FromUnixTimeSeconds(existingRange.StartDate)
            //         .UtcDateTime;
            //     var exEnd = DateTimeOffset.FromUnixTimeSeconds(existingRange.EndDate).UtcDateTime;

            //     // Comparamos con todos los nuevos rangos; basta con que uno se solape
            //     foreach (var newRange in newRanges)
            //     {
            //         // condición clásica de solapamiento: newStart < exEnd && newEnd > exStart
            //         if (newRange.Start < exEnd && newRange.End > exStart)
            //         {
            //             reservationOverlaps = true;
            //             break;
            //         }
            //     }

            //     if (reservationOverlaps)
            //     {
            //         break;
            //     }
            // }

            if (reservationOverlaps)
            {
                overlappingReservationIds.Add(existing.Id);
            }
        }

        // Ahora sumamos la nueva reserva como 1 unidad adicional por cada nueva reserva que se va a crear.
        // Nota: si la validación aplica por reserva y request puede contener múltiples TimeRanges que
        // correspondan a una sola reserva nueva, se considera la nueva reserva como 1. Si quieres tratar
        // cada timeRange de request como reserva separada, habría que ajustar la lógica.
        var totalConcurrentReservations = overlappingReservationIds.Count() + 1; // +1 por la reserva que se intenta crear

        if (totalConcurrentReservations > 5)
        {
            throw new Exception(
                "No puedes tener más de 5 reservas activas solapadas al mismo tiempo."
            );
        }

        var reservation = new Reservation
        {
            EnvironmentId = environment.Id,
            OwnerId = environment.OwnerId,
            RenterId = request.RenterId,
            IsInstant = environment.InstantBooking,
            Status = "pending",
            CreatedAt = DateTimeOffset.UtcNow.ToOffset(new TimeSpan(-4, 0, 0)).ToUnixTimeSeconds(),
            Currency = request.Currency,
            TotalPrice = request.TotalPrice,
            PeopleQuantity = request.PeopleQuantity,
            Payments = [],
            TimeRanges =
            [
                .. request.TimeRanges.Select(tr => new ReservationTimeRange
                {
                    StartDate = tr.StartDate,
                    EndDate = tr.EndDate,
                }),
            ],
        };

        if (!environment.InstantBooking)
        {
            var requestNotification = new SendToUserMessage
            {
                UserPublicId = environment.OwnerId,
                Title = "Nueva Solicitud de Reserva",
                Message = "Tienes una nueva solicitud de reserva de uno de tus ambientes",
                Type = "Info",
                Channel = "Push",
            };

            _notificationsPublisher.PublishSendToUser(requestNotification);
        }

        await _reservationRepo.AddAsync(reservation);
        await _reservationRepo.SaveChangesAsync();
        await transaction.CommitAsync();

        return _mapper.Map<ReservationResponse>(reservation);
    }
}
