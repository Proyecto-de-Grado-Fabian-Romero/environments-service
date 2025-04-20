using EnvironmentsService.Src.Domain.Entities;

namespace EnvironmentsService.src.Domain.Interfaces;

public interface IAreaRepository
{
    Task<List<Area>> GetAllAsync();
}
