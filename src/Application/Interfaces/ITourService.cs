using EnvironmentsService.Src.Domain.Entities.Tour360;

namespace EnvironmentsService.Src.Application.Interfaces;

public interface ITourService
{
    Task<Tour> CreateTourAsync(Guid environmentPublicId, List<Scene> scenes);

    Task<Tour?> GetTourByIdAsync(string id);
}
