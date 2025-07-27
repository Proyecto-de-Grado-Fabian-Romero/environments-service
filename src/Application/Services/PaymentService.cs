using System;
using EnvironmentsService.Src.Application.DTOs.GetRequest;
using EnvironmentsService.Src.Application.DTOs.Responses;
using EnvironmentsService.Src.Application.Interfaces;
using EnvironmentsService.Src.Domain.Interfaces;

namespace EnvironmentsService.Src.Application.Services;

public class PaymentService : IPaymentService
{
    private readonly IEnumerable<IPaymentGateway> _gateways;
    private readonly IReservationRepository _repo;

    public PaymentService(IEnumerable<IPaymentGateway> gateways, IReservationRepository repo)
    {
        _gateways = gateways;
        _repo = repo;
    }

    public async Task<PaymentUrlDto> CreatePayment(Guid reservationId, string gatewayName)
    {
        var reserva = await _repo.GetByPublicIdAsync(reservationId) ?? throw new Exception("Reservation not found");
        var request = new PaymentRequestDto
        {
            ReservationId = reserva.Id,
            ClientEmail = reserva.RenterEmail,
            // ClientFirstName = reserva.ClientFirstName,
            // ClientLastName = reserva.ClientLastName,
            ClientFullName = reserva.ClientFullName,
            ClientCI = reserva.ClientCI,
            ClientNIT = reserva.ClientNIT,
            TotalPrice = reserva.TotalPrice
        };

        var gateway = _gateways.FirstOrDefault(g => g.GatewayName.Equals(gatewayName, StringComparison.OrdinalIgnoreCase));
        if (gateway == null) throw new Exception("Payment gateway not supported");

        return await gateway.GeneratePaymentUrlAsync(request);
    }
}

