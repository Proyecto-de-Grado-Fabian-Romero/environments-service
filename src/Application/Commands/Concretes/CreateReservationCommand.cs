namespace EnvironmentsService.Src.Application.Commands.Concretes;

using AutoMapper;
using EnvironmentsService.Src.Application.Commands.Interfaces;
using EnvironmentsService.Src.Application.DTOs.Create;
using EnvironmentsService.Src.Application.DTOs.Responses;
using EnvironmentsService.Src.Domain.Entities.Booking;
using EnvironmentsService.Src.Domain.Interfaces;

public class CreateReservationCommand(
    CreateReservationDto request,
    IEnvironmentRepository environmentRepo,
    IReservationRepository reservationRepo,
    IMapper mapper
) : ICommand<ReservationResponse>
{
    private readonly IEnvironmentRepository _environmentRepo = environmentRepo;
    private readonly IReservationRepository _reservationRepo = reservationRepo;
    private readonly IMapper _mapper = mapper;

    public async Task<ReservationResponse> ExecuteAsync()
    {
        using var transaction = await _reservationRepo.BeginTransactionAsync();

        var environment = await _environmentRepo.GetSingleEnvironment(request.EnvironmentId)
            ?? throw new Exception("El entorno no existe.");

        foreach (var range in request.TimeRanges)
        {
            bool isOverlapping = await _reservationRepo.ExistsOverlappingReservationAsync(
                environment.Id,
                range.StartDate,
                range.EndDate);

            if (isOverlapping)
            {
                throw new Exception("Ya hay una reserva que se superpone con el rango solicitado.");
            }

            var startDateTime = DateTimeOffset.FromUnixTimeMilliseconds(range.StartDate).DateTime;
            var endDateTime = DateTimeOffset.FromUnixTimeMilliseconds(range.EndDate).DateTime;

            for (var date = startDateTime.Date; date <= endDateTime.Date; date = date.AddDays(1))
            {
                var resStart = date == startDateTime.Date ? startDateTime.TimeOfDay.TotalMilliseconds : 0;
                var resEnd = date == endDateTime.Date ? endDateTime.TimeOfDay.TotalMilliseconds : 86400000;

                var special = environment.SpecialAvailabilities.FirstOrDefault(sa => sa.Date.Date == date.Date);
                if (special != null)
                {
                    if (!special.IsAvailable)
                    {
                        throw new Exception($"El entorno no está disponible el {date:yyyy-MM-dd}.");
                    }

                    if (resStart < special.StartTime || resEnd > special.EndTime)
                    {
                        throw new Exception($"La reserva en {date:yyyy-MM-dd} debe estar entre {TimeSpan.FromMilliseconds(special.StartTime)} y {TimeSpan.FromMilliseconds(special.EndTime)}.");
                    }
                }
                else if (environment.RentalUnit == "Horas")
                {
                    var dayOfWeek = (int)date.DayOfWeek;
                    var weekly = environment.WeeklySchedules.FirstOrDefault(ws => ws.DayOfWeek == dayOfWeek)
                        ?? throw new Exception($"El entorno no está disponible el día {date:dddd}.");

                    if (resStart < weekly.StartTime || resEnd > weekly.EndTime)
                    {
                        throw new Exception($"La reserva en {date:yyyy-MM-dd} debe estar entre {TimeSpan.FromMilliseconds(weekly.StartTime)} y {TimeSpan.FromMilliseconds(weekly.EndTime)}.");
                    }
                }
            }
        }

        // Verificar límite de tiempo total
        var existingReservations = await _reservationRepo.GetActiveReservationsByRenterAsync(request.RenterId);
        int totalTimeUnits = 0;

        foreach (var res in existingReservations)
        {
            foreach (var range in res.TimeRanges)
            {
                var resStart = DateTimeOffset.FromUnixTimeMilliseconds(range.StartDate).UtcDateTime;
                var resEnd = DateTimeOffset.FromUnixTimeMilliseconds(range.EndDate).UtcDateTime;

                foreach (var newRange in request.TimeRanges)
                {
                    var newStart = DateTimeOffset.FromUnixTimeMilliseconds(newRange.StartDate).UtcDateTime;
                    var newEnd = DateTimeOffset.FromUnixTimeMilliseconds(newRange.EndDate).UtcDateTime;

                    if (newStart < resEnd && newEnd > resStart)
                    {
                        var duration = resEnd - resStart;
                        totalTimeUnits += environment.RentalUnit == "Días"
                            ? (int)Math.Ceiling(duration.TotalDays)
                            : (int)Math.Ceiling(duration.TotalHours);
                    }
                }
            }
        }

        foreach (var range in request.TimeRanges)
        {
            var duration = DateTimeOffset.FromUnixTimeMilliseconds(range.EndDate).UtcDateTime
                         - DateTimeOffset.FromUnixTimeMilliseconds(range.StartDate).UtcDateTime;

            totalTimeUnits += environment.RentalUnit == "Días"
                ? (int)Math.Ceiling(duration.TotalDays)
                : (int)Math.Ceiling(duration.TotalHours);
        }

        if (totalTimeUnits > 5)
        {
            throw new Exception("No puedes tener más de 5 unidades de tiempo reservadas activas.");
        }

        // Crear reserva con time ranges
        var reservation = new Reservation
        {
            EnvironmentId = environment.Id,
            OwnerId = environment.OwnerId,
            RenterId = request.RenterId,
            IsInstant = environment.InstantBooking,
            Status = environment.InstantBooking ? "confirmed" : "pending",
            CreatedAt = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
            Currency = request.Currency,
            TotalPrice = request.TotalPrice,
            Payments = [
                new ReservationPayment
                {
                    Amount = request.TotalPrice,
                    Currency = request.Currency,
                    Method = "QR",
                    Status = "paid",
                    PaidAt = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                }

            ],
            TimeRanges = [.. request.TimeRanges.Select(tr => new ReservationTimeRange
            {
                StartDate = tr.StartDate,
                EndDate = tr.EndDate,
            })],
        };

        await _reservationRepo.AddAsync(reservation);
        await _reservationRepo.SaveChangesAsync();
        await transaction.CommitAsync();

        return _mapper.Map<ReservationResponse>(reservation);
    }
}
