using EnvironmentsService.Src.Application.DTOs.Get;

namespace EnvironmentsService.Src.Application.Interfaces;

public interface IServiceService
{
    Task<List<ServiceDto>> GetAllAsync();
}
