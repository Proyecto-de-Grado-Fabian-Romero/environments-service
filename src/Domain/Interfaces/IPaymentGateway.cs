using EnvironmentsService.Src.Application.DTOs.GetRequest;
using EnvironmentsService.Src.Application.DTOs.Responses;

namespace EnvironmentsService.Src.Domain.Interfaces;

public interface IPaymentGateway
{
    string GatewayName { get; } // Eg: "Libelula", "Stripe", etc.

    Task<PaymentUrlDto> GeneratePaymentUrlAsync(PaymentRequestDto request);

    Task<PaymentStatusResponse> CheckPaymentStatusAsync(string identificador);
}
