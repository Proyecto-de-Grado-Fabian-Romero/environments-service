namespace EnvironmentsService.Src.Infraestructure.Repositories;

using Microsoft.EntityFrameworkCore;
using EnvironmentsService.Src.Domain.Entities;
using EnvironmentsService.Src.Domain.Interfaces;

public class TypeRepository : ITypeRepository
{
    private readonly DbContext _context;

    public TypeRepository(DbContext context)
    {
        _context = context;
    }

    public async Task<Guid> GetIdByPublicKeyAsync(string publicKey)
    {
        var type = await _context.Set<EnvironmentType>()
            .FirstOrDefaultAsync(t => t.PublicKey == publicKey);

        if (type == null)
        {
            throw new Exception($"EnvironmentType with publicKey '{publicKey}' not found.");
        }

        return type.Id;
    }
}
