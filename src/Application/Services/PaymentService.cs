using EnvironmentsService.Src.Application.DTOs.Create;
using EnvironmentsService.Src.Application.DTOs.GetRequest;
using EnvironmentsService.Src.Application.DTOs.Responses;
using EnvironmentsService.Src.Application.Interfaces;
using EnvironmentsService.Src.Domain.Interfaces;

namespace EnvironmentsService.Src.Application.Services;

public class PaymentService(IPaymentGateway gateway, IReservationRepository repo) : IPaymentService
{
    private readonly IReservationRepository _repo = repo;
    private readonly IPaymentGateway _gateway = gateway;

    public async Task<PaymentUrlDto> CreatePayment(
        CreatePaymentWithClientDto dto,
        string fechaVencimiento
    )
    {
        var reservation =
            await _repo.GetByPublicIdAsync(dto.ReservationId)
            ?? throw new Exception("Reservation not found");

        var request = new PaymentRequestDto
        {
            ReservationId = reservation.Id,
            ClientEmail = dto.ClientEmail,
            ClientCI = dto.ClientCI,
            ClientNIT = dto.ClientNIT,
            ClientFullName = dto.ClientFullName,
            TotalPrice = reservation.TotalPrice,
        };

        return await _gateway.GeneratePaymentUrlAsync(request, fechaVencimiento);
    }

    public async Task<PaymentStatusResponse> CheckAndUpdatePaymentAsync(Guid reservationId)
    {
        var reservation =
            await _repo.GetByIdAsync(reservationId) ?? throw new Exception("Reservation not found");

        // var result = await _gateway.CheckPaymentStatusAsync(reservation.Id.ToString());

        // await _repo.AddPaymentAsync(reservation.PublicId, new ReservationPayment
        // {
        //     Amount = reservation.TotalPrice,
        //     Currency = reservation.Currency,
        //     Method = "QR",
        //     Status = "paid",
        //     PaidAt = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
        // });
        // await _repo.MarkPaymentAsPaidAsync(reservation.PublicId, "Libelula", DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());
        // await _repo.SaveChangesAsync();

        // if (result.Status == "paid")
        // {
        // await _repo.MarkPaymentAsPaidAsync(reservation.Id, "QR", DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());
        reservation.Status = "paid";
        await _repo.SaveChangesAsync();
        return new PaymentStatusResponse { Status = "paid", InvoiceUrl = string.Empty };
    }
}
