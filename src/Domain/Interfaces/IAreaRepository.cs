using EnvironmentsService.Src.Domain.Entities;

namespace EnvironmentsService.Src.Domain.Interfaces;

public interface IAreaRepository
{
    Task<List<Area>> GetAllAsync();
}
