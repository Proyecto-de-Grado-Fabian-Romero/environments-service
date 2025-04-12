using EnvironmentsService.Src.Application.DTOs.GetRequest;
using EnvironmentsService.src.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace EnvironmentsService.src.Infraestructure.Repositories;

public class EnvironmentRepository(DbContext context) : IEnvironmentRepository
{
    private readonly DbContext _context = context;

    public async Task<List<Src.Domain.Entities.Environment>> FilterEnvironmentsAsync(GetAvailableEnvironmentsRequest request)
    {
        var query = _context.Set<Src.Domain.Entities.Environment>()
            .Include(e => e.Type)
            .Include(e => e.PricingPolicies)
            .Include(e => e.Photos)
            .Include(e => e.EnvironmentServices).ThenInclude(es => es.Service)
            .Include(e => e.EnvironmentAreas).ThenInclude(ea => ea.Area)
            .Include(e => e.NonAvailabilities)
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

        if (request.ServicePublicKeys?.Any() == true)
        {
            query = query.Where(e =>
                request.ServicePublicKeys.All(reqKey =>
                    e.EnvironmentServices.Any(es => es.Service.PublicKey == reqKey)));
        }

        if (request.Areas?.Any() == true)
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

        return await query.ToListAsync();
    }
}
