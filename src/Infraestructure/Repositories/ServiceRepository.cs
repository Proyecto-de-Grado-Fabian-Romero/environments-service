using EnvironmentsService.Src.Domain.Entities;
using EnvironmentsService.Src.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace EnvironmentsService.Src.Infraestructure.Repositories;

public class ServiceRepository(DbContext context) : IServiceRepository
{
    private readonly DbContext _context = context;

    public async Task<List<Service>> GetAllAsync()
    {
        return await _context.Set<Service>().ToListAsync();
    }

    public async Task<Dictionary<string, Guid>> GetIdsByPublicKeysAsync(List<string> publicKeys)
    {
        return await _context.Set<Service>()
            .Where(s => publicKeys.Contains(s.PublicKey))
            .ToDictionaryAsync(s => s.PublicKey, s => s.Id);
    }
}
