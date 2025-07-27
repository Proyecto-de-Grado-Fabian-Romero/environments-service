using EnvironmentsService.Src.Application.DTOs.Responses;

namespace EnvironmentsService.Src.Application.Interfaces;

public interface IPaymentService
{
    Task<PaymentUrlDto> CreatePayment(Guid reservationId, string gatewayName);
}
