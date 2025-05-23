namespace EnvironmentsService.Src.Application.Commands.Concretes;

using AutoMapper;
using EnvironmentsService.Src.Application.Commands.Interfaces;
using EnvironmentsService.Src.Application.DTOs.Create;
using EnvironmentsService.Src.Application.DTOs.Responses;
using EnvironmentsService.Src.Domain.Entities.Booking;
using EnvironmentsService.Src.Domain.Interfaces;

public class CreateReservationCommand(
    CreateReservationRequest request,
    IEnvironmentRepository environmentRepo,
    IReservationRepository reservationRepo,
    IMapper mapper
) : ICommand<ReservationResponse>
{
    private readonly IEnvironmentRepository _environmentRepo = environmentRepo;
    private readonly IMapper _mapper = mapper;

    public async Task<ReservationResponse> ExecuteAsync()
    {
        using var transaction = await reservationRepo.BeginTransactionAsync();

        var environment = await _environmentRepo.GetSingleEnvironment(request.EnvironmentId)
            ?? throw new Exception("El entorno no existe.");

        bool isOverlapping = await reservationRepo.ExistsOverlappingReservationAsync(
            environment.Id,
            request.StartDate,
            request.EndDate);

        if (isOverlapping)
        {
            throw new Exception("El entorno ya tiene una reserva en ese rango de fechas.");
        }

        var startDateTime = DateTimeOffset.FromUnixTimeMilliseconds(request.StartDate).DateTime;
        var endDateTime = DateTimeOffset.FromUnixTimeMilliseconds(request.EndDate).DateTime;

        for (var date = startDateTime.Date; date <= endDateTime.Date; date = date.AddDays(1))
        {
            var special = environment.SpecialAvailabilities.FirstOrDefault(sa => sa.Date.Date == date.Date);

            if (special != null)
            {
                if (!special.IsAvailable)
                {
                    throw new Exception($"El entorno no está disponible el {date:yyyy-MM-dd}.");
                }

                var resStart = date == startDateTime.Date ? startDateTime.TimeOfDay.TotalMilliseconds : 0;
                var resEnd = date == endDateTime.Date ? endDateTime.TimeOfDay.TotalMilliseconds : 86400000;

                if (resStart < special.StartTime || resEnd > special.EndTime)
                {
                    throw new Exception($"La reserva en {date:yyyy-MM-dd} debe estar entre {TimeSpan.FromMilliseconds(special.StartTime)} y {TimeSpan.FromMilliseconds(special.EndTime)}.");
                }
            }
            else
            {
                var dayOfWeek = (int)date.DayOfWeek;
                var weekly = environment.WeeklySchedules.FirstOrDefault(ws => ws.DayOfWeek == dayOfWeek);
                if (weekly == null)
                {
                    throw new Exception($"El entorno no está disponible el día {date:dddd}.");
                }

                var resStart = date == startDateTime.Date ? startDateTime.TimeOfDay.TotalMilliseconds : 0;
                var resEnd = date == endDateTime.Date ? endDateTime.TimeOfDay.TotalMilliseconds : 86400000;

                if (resStart < weekly.StartTime || resEnd > weekly.EndTime)
                {
                    throw new Exception($"La reserva en {date:yyyy-MM-dd} debe estar entre {TimeSpan.FromMilliseconds(weekly.StartTime)} y {TimeSpan.FromMilliseconds(weekly.EndTime)}.");
                }
            }
        }

        var reservations = await reservationRepo.GetActiveReservationsByRenterAsync(request.RenterId);

        int totalTimeUnits = 0;

        foreach (var res in reservations)
        {
            var resStart = DateTimeOffset.FromUnixTimeMilliseconds(res.StartDate).UtcDateTime;
            var resEnd = DateTimeOffset.FromUnixTimeMilliseconds(res.EndDate).UtcDateTime;

            var newStartDt = DateTimeOffset.FromUnixTimeMilliseconds(request.StartDate).UtcDateTime;
            var newEndDt = DateTimeOffset.FromUnixTimeMilliseconds(request.EndDate).UtcDateTime;

            if (newStartDt < resEnd && newEndDt > resStart)
            {
                var duration = resEnd - resStart;
                if (res.Environment.RentalUnit == "Días")
                {
                    totalTimeUnits += (int)Math.Ceiling(duration.TotalDays);
                }
                else
                {
                    totalTimeUnits += (int)Math.Ceiling(duration.TotalHours);
                }
            }
        }

        var newDuration = DateTimeOffset.FromUnixTimeMilliseconds(request.EndDate).UtcDateTime
                        - DateTimeOffset.FromUnixTimeMilliseconds(request.StartDate).UtcDateTime;

        if (environment.RentalUnit == "Días")
        {
            totalTimeUnits += (int)Math.Ceiling(newDuration.TotalDays);
        }
        else
        {
            totalTimeUnits += (int)Math.Ceiling(newDuration.TotalHours);
        }

        if (totalTimeUnits > 5)
        {
            throw new Exception("No puedes tener más de 5 unidades de tiempo reservadas al mismo tiempo.");
        }

        var reservation = new Reservation
        {
            EnvironmentId = environment.Id,
            OwnerId = environment.OwnerId,
            RenterId = request.RenterId,
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            IsInstant = environment.InstantBooking,
            Status = environment.InstantBooking ? "confirmed" : "pending",
            CreatedAt = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
            Payments =
            [
                new ReservationPayment
                {
                    Amount = 100,
                    Currency = "BOB",
                    Method = "QR",
                    Status = "paid",
                    PaidAt = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                },
            ],
        };

        await reservationRepo.AddAsync(reservation);
        await reservationRepo.SaveChangesAsync();

        await transaction.CommitAsync();

        return _mapper.Map<ReservationResponse>(reservation);
    }
}
