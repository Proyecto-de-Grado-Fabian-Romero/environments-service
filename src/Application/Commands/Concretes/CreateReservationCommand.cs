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
    private readonly IReservationRepository _reservationRepo = reservationRepo;
    private readonly IMapper _mapper = mapper;

    public async Task<ReservationResponse> ExecuteAsync()
    {
        using var transaction = await _reservationRepo.BeginTransactionAsync();

        var environment = await _environmentRepo.GetSingleEnvironment(request.EnvironmentId)
            ?? throw new Exception("El entorno no existe.");

        bool isOverlapping = await _reservationRepo.ExistsOverlappingReservationAsync(
            environment.Id,
            request.StartDate,
            request.EndDate);

        if (isOverlapping)
        {
            throw new Exception("El entorno ya tiene una reserva en ese rango de fechas.");
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

        await _reservationRepo.AddAsync(reservation);
        await _reservationRepo.SaveChangesAsync();

        await transaction.CommitAsync();

        return _mapper.Map<ReservationResponse>(reservation);
    }
}
