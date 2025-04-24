using EnvironmentsService.Src.Domain.Entities;

namespace EnvironmentsService.Src.Domain.Interfaces;

public interface IServiceRepository
{
    Task<List<Service>> GetAllAsync();

    Task<Dictionary<string, Guid>> GetIdsByPublicKeysAsync(List<string> publicKeys);
}
