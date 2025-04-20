using EnvironmentsService.Src.Application.DTOs.Get;

namespace EnvironmentsService.src.Application.Interfaces;

public interface IAreaService
{
    Task<List<AreaDto>> GetAllAsync();
}
