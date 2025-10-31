using EnvironmentsService.Src.Application.DTOs.Create;
using EnvironmentsService.Src.Application.DTOs.GetRequest;
using EnvironmentsService.Src.Application.DTOs.Responses;
using EnvironmentsService.Src.Application.Interfaces;
using EnvironmentsService.Src.Domain.Entities.Booking;
using EnvironmentsService.Src.Domain.Interfaces;

namespace EnvironmentsService.Src.Application.Services;

public class PaymentService(IPaymentGateway gateway, IReservationRepository repo) : IPaymentService
{
    private readonly IReservationRepository _repo = repo;
    private readonly IPaymentGateway _gateway = gateway;

    public async Task<PaymentUrlDto> CreatePayment(CreatePaymentWithClientDto dto, string gatewayName)
    {
        var reservation = await _repo.GetByPublicIdAsync(dto.ReservationId) ?? throw new Exception("Reservation not found");
        await _repo.AddPaymentAsync(reservation.PublicId, new ReservationPayment
        {
            Amount = reservation.TotalPrice,
            Currency = reservation.Currency,
            Method = "QR",
            Status = "paid",
            PaidAt = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
        });
        await _repo.MarkPaymentAsPaidAsync(reservation.PublicId, "Libelula", DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());
        await _repo.SaveChangesAsync();

        var request = new PaymentRequestDto
        {
            ReservationId = reservation.Id,
            ClientEmail = dto.ClientEmail,
            ClientCI = dto.ClientCI,
            ClientNIT = dto.ClientNIT,
            ClientFullName = dto.ClientFullName,
            TotalPrice = reservation.TotalPrice,
        };

        return await _gateway.GeneratePaymentUrlAsync(request);
    }

    public async Task<PaymentStatusResponse> CheckAndUpdatePaymentAsync(Guid reservationId)
    {
        var reservation = await _repo.GetByPublicIdAsync(reservationId)
            ?? throw new Exception("Reservation not found");

        var payment = reservation.Payments.FirstOrDefault(p => p.Status == "pending");
        if (payment == null)
        {
            throw new Exception("No pending payment found");
        }

        var result = await _gateway.CheckPaymentStatusAsync(reservation.Id.ToString());

        if (result.Status == "paid")
        {
            await _repo.MarkPaymentAsPaidAsync(reservation.Id, payment.Method, result.PaidAt!);
            reservation.Status = "paid";
            await _repo.SaveChangesAsync();
        }

        return result;
    }
}
