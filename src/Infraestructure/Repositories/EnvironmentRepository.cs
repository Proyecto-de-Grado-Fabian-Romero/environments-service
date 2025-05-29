using EnvironmentsService.Src.Application.DTOs.GetRequest;
using EnvironmentsService.Src.Application.Pipelines;
using EnvironmentsService.Src.Domain.Entities;
using EnvironmentsService.Src.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace EnvironmentsService.Src.Infraestructure.Repositories;

public class EnvironmentRepository(DbContext context, EnvironmentFilterPipeline pipeline) : IEnvironmentRepository
{
    private readonly DbContext _context = context;
    private readonly EnvironmentFilterPipeline _pipeline = pipeline;

    public async Task<(List<Domain.Entities.Environment> Environments, int TotalItems)> FilterEnvironmentsAsync(
    GetAvailableEnvironmentsRequest request, int page, int limit)
    {
        var baseQuery = GetBaseEnvironmentQuery();
        var filteredQuery = _pipeline.ApplyFilters(baseQuery, request);

        var totalItems = await filteredQuery.CountAsync();
        var environments = await filteredQuery
            .Skip((page - 1) * limit)
            .Take(limit)
            .ToListAsync();

        return (environments, totalItems);
    }

    public async Task<(List<Domain.Entities.Environment> Environments, int TotalItems)> GetOwnerEnvironmentsAsync(
        Guid pubUserId, int page, int limit)
    {
        var baseQuery = GetBaseEnvironmentQuery()
            .Where(e => e.OwnerId == pubUserId);

        var totalItems = await baseQuery.CountAsync();

        var environments = totalItems == 0
            ? []
            : await baseQuery
                .Skip((page - 1) * limit)
                .Take(limit)
                .ToListAsync();

        return (environments, totalItems);
    }

    public async Task<Domain.Entities.Environment?> GetSingleEnvironment(Guid publicId)
    {
        return await GetBaseEnvironmentQuery()
            .FirstOrDefaultAsync(e => e.PublicId == publicId);
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
        var environment = await _context.Set<Domain.Entities.Environment>()
            .FirstOrDefaultAsync(e => e.PublicId == publicId) ?? throw new Exception("Environment not found");
        environment.Equipment = serializedEquipment;

        await _context.SaveChangesAsync();
    }

    private IQueryable<Domain.Entities.Environment> GetBaseEnvironmentQuery()
    {
        return _context.Set<Domain.Entities.Environment>()
            .Include(e => e.Type)
            .Include(e => e.PricingPolicies)
            .Include(e => e.DiscountPolicies)
            .Include(e => e.Photos)
            .Include(e => e.EnvironmentServices).ThenInclude(es => es.Service)
            .Include(e => e.EnvironmentAreas).ThenInclude(ea => ea.Area)
            .Include(e => e.WeeklySchedules)
            .Include(e => e.NonAvailabilities)
            .Where(e => !e.Deleted && !e.Hidden)
            .AsQueryable();
    }
}
