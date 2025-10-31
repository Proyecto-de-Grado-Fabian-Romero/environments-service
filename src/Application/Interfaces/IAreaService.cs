using EnvironmentsService.Src.Application.DTOs.Get;

namespace EnvironmentsService.Src.Application.Interfaces;

public interface IAreaService
{
    Task<List<AreaDto>> GetAllAsync();
}
