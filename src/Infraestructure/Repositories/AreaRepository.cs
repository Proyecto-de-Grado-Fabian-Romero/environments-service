using EnvironmentsService.Src.Domain.Entities;
using EnvironmentsService.src.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace EnvironmentsService.src.Infraestructure.Repositories;

public class AreaRepository(DbContext context) : IAreaRepository
{
    private readonly DbContext _context = context;

    public async Task<List<Area>> GetAllAsync()
    {
        return await _context.Set<Area>().ToListAsync();
    }
}
