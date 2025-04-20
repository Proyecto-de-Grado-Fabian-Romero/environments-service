using EnvironmentsService.Src.Application.DTOs.GetRequest;
using EnvironmentsService.Src.Application.Pipelines;
using EnvironmentsService.src.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace EnvironmentsService.src.Infraestructure.Repositories;

public class EnvironmentRepository(DbContext context, EnvironmentFilterPipeline pipeline) : IEnvironmentRepository
{
    private readonly DbContext _context = context;
    private readonly EnvironmentFilterPipeline _pipeline = pipeline;

    public async Task<(List<Src.Domain.Entities.Environment> Environments, int TotalItems)> FilterEnvironmentsAsync(
        GetAvailableEnvironmentsRequest request, int page, int limit)
    {
        var baseQuery = _context.Set<Src.Domain.Entities.Environment>()
            .Include(e => e.Type)
            .Include(e => e.PricingPolicies)
            .Include(e => e.Photos)
            .Include(e => e.EnvironmentServices).ThenInclude(es => es.Service)
            .Include(e => e.EnvironmentAreas).ThenInclude(ea => ea.Area)
            .Include(e => e.NonAvailabilities)
            .Where(e => e.Deleted == false)
            .Where(e => e.Hidden == false)
            .AsQueryable();

        var filteredQuery = _pipeline.ApplyFilters(baseQuery, request);

        var totalItems = await filteredQuery.CountAsync();

        var environments = await filteredQuery
            .Skip((page - 1) * limit)
            .Take(limit)
            .ToListAsync();

        return (environments, totalItems);
    }

    public async Task<Src.Domain.Entities.Environment?> GetSingleEnvironment(Guid publicId)
    {
        return await _context.Set<Src.Domain.Entities.Environment>()
            .Include(e => e.Type)
            .Include(e => e.PricingPolicies)
            .Include(e => e.Photos)
            .Include(e => e.DiscountPolicies)
            .Include(e => e.WeeklySchedules)
            .Include(e => e.SpecialAvailabilities)
            .Include(e => e.EnvironmentServices).ThenInclude(es => es.Service)
            .Include(e => e.EnvironmentAreas).ThenInclude(ea => ea.Area)
            .Include(e => e.NonAvailabilities)
            .Where(e => e.Deleted == false && e.Hidden == false)
            .FirstOrDefaultAsync(e => e.PublicId == publicId);
    }
}
