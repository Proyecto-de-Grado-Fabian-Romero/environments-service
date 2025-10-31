using EnvironmentsService.Src.Application.DTOs.Create;
using EnvironmentsService.Src.Application.DTOs.Responses;

namespace EnvironmentsService.Src.Application.Interfaces;

public interface IPaymentService
{
    Task<PaymentUrlDto> CreatePayment(CreatePaymentWithClientDto dto, string fechaVencimiento);

    Task<PaymentStatusResponse> CheckAndUpdatePaymentAsync(Guid reservationId);
}
