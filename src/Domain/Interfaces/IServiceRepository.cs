using EnvironmentsService.Src.Domain.Entities;

namespace EnvironmentsService.src.Domain.Interfaces;

public interface IServiceRepository
{
    Task<Service> GetServicesAsync();
}
