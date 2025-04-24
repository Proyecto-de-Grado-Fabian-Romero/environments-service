using EnvironmentsService.Src.Domain.Entities.Tour360;

namespace EnvironmentsService.Src.Domain.Interfaces;

public interface ITourRepository
{
    Task SaveAsync(Tour tour);

    Task<Tour?> GetByIdAsync(string id);
}
