using EnvironmentsService.Src.Domain.Entities;
using EnvironmentsService.Src.Domain.Entities.Booking;
using EnvironmentsService.Src.Domain.Interfaces;
using EnvironmentsService.Src.Infraestructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EnvironmentsService.Src.Infraestructure.Repositories;

public class AvailabilityRepository(AppDbContext context) : IAvailabilityRepository
{
    private readonly AppDbContext _context = context;

    public async Task<IEnumerable<NonAvailability>> GetNonAvailabilitiesAsync(Guid environmentId, DateTime start, DateTime end)
    {
        var startUnix = new DateTimeOffset(start).ToUnixTimeSeconds();
        var endUnix = new DateTimeOffset(end).ToUnixTimeSeconds();

        return await _context.NonAvailabilities
            .Where(na => na.EnvironmentId == environmentId &&
                         na.StartDate <= endUnix &
                         na.EndDate >= startUnix)
            .ToListAsync();
    }

    public async Task<IEnumerable<Reservation>> GetReservationsAsync(Guid environmentId, DateTime start, DateTime end)
    {
        var startUnix = new DateTimeOffset(start).ToUnixTimeSeconds();
        var endUnix = new DateTimeOffset(end).ToUnixTimeSeconds();

        var excludedStatuses = new[] { "rejected", "cancelled" };

        return await _context.Reservations
            .Where(r => r.EnvironmentId == environmentId &&
                        r.StartDate <= endUnix &&
                        r.EndDate >= startUnix &&
                        !excludedStatuses.Contains(r.Status))
            .ToListAsync();
    }
}
