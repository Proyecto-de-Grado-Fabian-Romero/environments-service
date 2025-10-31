using EnvironmentsService.Src.Domain.Entities;
using EnvironmentsService.Src.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace EnvironmentsService.Src.Infraestructure.Repositories;

public class AreaRepository(DbContext context) : IAreaRepository
{
    private readonly DbContext _context = context;

    public async Task<List<Area>> GetAllAsync()
    {
        return await _context.Set<Area>().ToListAsync();
    }

    public async Task<Dictionary<string, Guid>> GetIdsByPublicKeysAsync(List<string> publicKeys)
    {
        return await _context.Set<Area>()
            .Where(a => publicKeys.Contains(a.PublicKey))
            .ToDictionaryAsync(a => a.PublicKey, a => a.Id);
    }
}
