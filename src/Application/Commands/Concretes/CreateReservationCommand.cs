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
    public async Task<ReservationResponse> ExecuteAsync()
    {
        var environment = await environmentRepo.GetSingleEnvironment(request.EnvironmentId)
            ?? throw new Exception("El entorno no existe.");

        bool isOverlapping = environment.Reservations.Any(r =>
            r.Status != "cancelled" &&
            request.StartDate < r.EndDate &&
            request.EndDate > r.StartDate);

        if (isOverlapping)
        {
            throw new Exception("El entorno ya está reservado en ese rango de fechas.");
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
                    Amount = 100.00m,
                    Currency = "USD",
                    Method = "manual",
                    Status = "paid",
                    PaidAt = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                },
            ],
        };

        await reservationRepo.AddAsync(reservation);
        await reservationRepo.SaveChangesAsync();

        return mapper.Map<ReservationResponse>(reservation);
    }
}
