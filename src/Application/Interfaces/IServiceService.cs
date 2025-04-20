using EnvironmentsService.Src.Application.DTOs.Get;

namespace EnvironmentsService.src.Application.Interfaces;

public interface IServiceService
{
    Task<ServiceDto> GetAllServicesAsync();
}
