using EnvironmentsService.Src.Application.DTOs.GetRequest;
using EnvironmentsService.src.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace EnvironmentsService.src.Infraestructure.Repositories;

public class EnvironmentRepository(DbContext context) : IEnvironmentRepository
{
    private readonly DbContext _context = context;

    public async Task<(List<Src.Domain.Entities.Environment> Environments, int TotalItems)> FilterEnvironmentsAsync(
    GetAvailableEnvironmentsRequest request, int page, int limit)
    {
        var query = _context.Set<Src.Domain.Entities.Environment>()
            .Include(e => e.Type)
            .Include(e => e.PricingPolicies)
            .Include(e => e.Photos)
            .Include(e => e.EnvironmentServices).ThenInclude(es => es.Service)
            .Include(e => e.EnvironmentAreas).ThenInclude(ea => ea.Area)
            .Include(e => e.NonAvailabilities)
            .Where(e => !e.Deleted && !e.Hidden)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.Location))
        {
            query = query.Where(e => e.Location.Contains(request.Location));
        }

        if (!string.IsNullOrWhiteSpace(request.EnvironmentTypePublicKey))
        {
            query = query.Where(e => e.Type.PublicKey == request.EnvironmentTypePublicKey);
        }

        if (request.InstantBookingRequired.HasValue)
        {
            query = query.Where(e => e.InstantBooking == request.InstantBookingRequired);
        }

        if (request.MinCapacity.HasValue)
        {
            query = query.Where(e => e.Capacity >= request.MinCapacity);
        }

        if (request.MinPrice.HasValue || request.MaxPrice.HasValue)
        {
            query = query.Where(e =>
                e.PricingPolicies.Any(p =>
                    (!request.MinPrice.HasValue || p.BasePrice >= request.MinPrice) &&
                    (!request.MaxPrice.HasValue || p.BasePrice <= request.MaxPrice)));
        }

        if (request.ServicePublicKeys?.Count > 0)
        {
            query = query.Where(e =>
                request.ServicePublicKeys.All(reqKey =>
                    e.EnvironmentServices.Any(es => es.Service.PublicKey == reqKey)));
        }

        if (request.Areas?.Count > 0)
        {
            foreach (var (areaPublicKey, minQuantity) in request.Areas)
            {
                query = query.Where(e =>
                    e.EnvironmentAreas.Any(ea =>
                        ea.Area.PublicKey == areaPublicKey && ea.Quantity >= minQuantity));
            }
        }

        if (request.StartDate.HasValue && request.EndDate.HasValue)
        {
            query = query.Where(e => !e.NonAvailabilities.Any(n =>
                n.StartDate <= request.StartDate && n.EndDate >= request.EndDate));
        }

        var totalItems = await query.CountAsync();

        var environments = await query
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
