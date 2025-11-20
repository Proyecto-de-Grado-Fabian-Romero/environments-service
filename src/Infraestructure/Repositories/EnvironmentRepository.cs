using EnvironmentsService.Src.Application.DTOs.GetRequest;
using EnvironmentsService.Src.Application.Pipelines;
using EnvironmentsService.Src.Domain.Entities;
using EnvironmentsService.Src.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace EnvironmentsService.Src.Infraestructure.Repositories;

public class EnvironmentRepository(
    DbContext context,
    EnvironmentFilterPipeline pipeline,
    EnvironmentPostFilterPipeline postPipeline
) : IEnvironmentRepository
{
    private readonly DbContext _context = context;
    private readonly EnvironmentFilterPipeline _pipeline = pipeline;
    private readonly EnvironmentPostFilterPipeline _postPipeline = postPipeline;

    public async Task<(
        List<Domain.Entities.Environment> Environments,
        int TotalItems
    )> FilterEnvironmentsAsync(
        GetAvailableEnvironmentsRequest request,
        int page,
        int limit,
        Guid? userPublicId
    )
    {
        var baseQuery = GetBaseEnvironmentQuery();

        if (userPublicId.HasValue)
        {
            baseQuery = baseQuery.Where(e => e.OwnerId != userPublicId.Value);
        }

        baseQuery.Where(e => !e.Hidden);

        var filteredQuery = _pipeline.ApplyFilters(baseQuery, request);

        var resultsFromDb = await filteredQuery.ToListAsync();

        var filteredInMemory = _postPipeline.ApplyFilters(resultsFromDb, request);

        var paged = filteredInMemory.Skip((page - 1) * limit).Take(limit).ToList();

        return (paged, filteredInMemory.Count());
    }

    public async Task<(
        List<Domain.Entities.Environment> Environments,
        int TotalItems
    )> GetOwnerEnvironmentsAsync(Guid pubUserId, int page, int limit)
    {
        var baseQuery = GetBaseEnvironmentQuery().Where(e => e.OwnerId == pubUserId);

        var totalItems = await baseQuery.CountAsync();

        var environments =
            totalItems == 0
                ? []
                : await baseQuery.Skip((page - 1) * limit).Take(limit).ToListAsync();

        return (environments, totalItems);
    }

    public async Task<Domain.Entities.Environment?> GetSingleEnvironment(Guid publicId)
    {
        return await GetBaseEnvironmentQuery()
            .FirstOrDefaultAsync(e => e.PublicId == publicId && !e.Deleted);
    }

    public async Task AddAsync(Domain.Entities.Environment environment)
    {
        await _context.Set<Domain.Entities.Environment>().AddAsync(environment);
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }

    public async Task AddImageAsync(EnvironmentPhoto image)
    {
        await _context.Set<EnvironmentPhoto>().AddAsync(image);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateDetectedEquipmentAsync(Guid publicId, string serializedEquipment)
    {
        var environment =
            await _context
                .Set<Domain.Entities.Environment>()
                .FirstOrDefaultAsync(e => e.PublicId == publicId)
            ?? throw new Exception("Environment not found");
        environment.Equipment = serializedEquipment;

        await _context.SaveChangesAsync();
    }

    public async Task<List<Domain.Entities.Environment>> GetFilteredEnvironmentsAsync(
        GetAvailableEnvironmentsRequest request
    )
    {
        var baseQuery = GetBaseEnvironmentQuery();
        var filteredQuery = _pipeline.ApplyFilters(baseQuery, request);

        return await filteredQuery.ToListAsync();
    }

    public async Task<EnvironmentPhoto?> GetPhotoByFileIdAsync(string fileId)
    {
        return await _context.Set<EnvironmentPhoto>().FirstOrDefaultAsync(p => p.FileId == fileId);
    }

    public async Task RemovePhotoAsync(EnvironmentPhoto photo)
    {
        _context.Set<EnvironmentPhoto>().Remove(photo);
        await _context.SaveChangesAsync();
    }

    public async Task<Domain.Entities.Environment?> GetByPublicIdWithIncludesAsync(Guid publicId)
    {
        return await _context
            .Set<Domain.Entities.Environment>()
            .Include(e => e.Type)
            .Include(e => e.PricingPolicies)
            .Include(e => e.DiscountPolicies)
            .Include(e => e.Photos)
            .Include(e => e.EnvironmentServices)
            .ThenInclude(es => es.Service)
            .Include(e => e.EnvironmentAreas)
            .ThenInclude(ea => ea.Area)
            .Include(e => e.WeeklySchedules)
            .Include(e => e.NonAvailabilities)
            .FirstOrDefaultAsync(e => e.PublicId == publicId && !e.Deleted);
    }

    private IQueryable<Domain.Entities.Environment> GetBaseEnvironmentQuery()
    {
        return _context
            .Set<Domain.Entities.Environment>()
            .Include(e => e.Type)
            .Include(e => e.PricingPolicies)
            .Include(e => e.DiscountPolicies)
            .Include(e => e.Photos)
            .Include(e => e.EnvironmentServices)
            .ThenInclude(es => es.Service)
            .Include(e => e.EnvironmentAreas)
            .ThenInclude(ea => ea.Area)
            .Include(e => e.WeeklySchedules)
            .Include(e => e.NonAvailabilities)
            .Where(e => !e.Deleted)
            .AsQueryable();
    }

    public async Task<bool> SetHiddenByPublicIdAsync(
        Guid environmentPublicId,
        bool hide,
        Guid userPublicId
    )
    {
        var env = await _context
            .Set<Domain.Entities.Environment>()
            .FirstOrDefaultAsync(e => e.PublicId == environmentPublicId);

        if (env == null)
        {
            return false;
        }

        if (env.OwnerId != userPublicId)
        {
            return false;
        }

        env.Hidden = hide;
        _context.Update(env);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> SoftDeleteByPublicIdAsync(Guid environmentPublicId, Guid userPublicId)
    {
        var env = await _context
            .Set<Domain.Entities.Environment>()
            .FirstOrDefaultAsync(e => e.PublicId == environmentPublicId);

        if (env == null)
        {
            return false;
        }

        if (env.OwnerId != userPublicId)
        {
            return false;
        }

        env.Deleted = true;
        _context.Update(env);
        await _context.SaveChangesAsync();
        return true;
    }
}
