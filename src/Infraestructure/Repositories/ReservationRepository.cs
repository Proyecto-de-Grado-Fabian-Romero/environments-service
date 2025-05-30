using EnvironmentsService.Src.Domain.Entities.Booking;
using EnvironmentsService.Src.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace EnvironmentsService.Src.Infraestructure.Repositories;

public class ReservationRepository(DbContext context) : IReservationRepository
{
    private readonly DbContext _context = context;

    public async Task AddAsync(Reservation reservation)
    {
        await _context.Set<Reservation>().AddAsync(reservation);
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }

    public async Task<IDbContextTransaction> BeginTransactionAsync()
    {
        return await _context.Database.BeginTransactionAsync();
    }

    public async Task<bool> ExistsOverlappingReservationAsync(Guid environmentId, long start, long end)
    {
        return await _context.Set<Reservation>()
            .Include(r => r.TimeRanges)
            .Where(r => r.EnvironmentId == environmentId &&
                        r.Status != "cancelled" &&
                        r.Status != "rejected")
            .AnyAsync(r =>
                r.TimeRanges.Any(tr =>
                    start < tr.EndDate && end > tr.StartDate));
    }

    public async Task<List<Reservation>> GetActiveReservationsByRenterAsync(Guid renterId)
    {
        return await _context.Set<Reservation>()
            .Include(r => r.Environment)
            .Include(r => r.TimeRanges)
            .Where(r =>
                r.RenterId == renterId &&
                r.Status != "cancelled" &&
                r.Status != "rejected")
            .ToListAsync();
    }

    public async Task<List<Reservation>> GetUserReservationsAsync(Guid userId, string status, int page, int limit)
    {
        var query = _context.Set<Reservation>()
            .Where(r => r.RenterId == userId && r.Status.ToLower() == status.ToLower())
            .Include(r => r.Environment)
                .ThenInclude(e => e.Photos)
            .Include(r => r.TimeRanges)
            .AsQueryable();

        return await query
            .OrderBy(r => r.TimeRanges.Min(tr => tr.StartDate))
            .Skip((page - 1) * limit)
            .Take(limit)
            .ToListAsync();
    }

    public async Task<Reservation?> GetByPublicIdAsync(Guid publicId)
    {
        return await _context.Set<Reservation>()
            .Include(r => r.Environment)
                .ThenInclude(e => e.Photos)
            .FirstOrDefaultAsync(r => r.PublicId == publicId);
    }

    public async Task<bool> ExistsOverlappingConfirmedAsync(
        Guid environmentId,
        Guid currentReservationId,
        ICollection<ReservationTimeRange> timeRanges)
    {
        var confirmed = await _context.Set<Reservation>()
            .Include(r => r.TimeRanges)
            .Where(r => r.EnvironmentId == environmentId &&
                        r.Status == "confirmed" &&
                        r.Id != currentReservationId)
            .ToListAsync();

        return confirmed.Any(conf =>
            conf.TimeRanges.Any(cRange =>
                timeRanges.Any(newRange =>
                    newRange.StartDate < cRange.EndDate &&
                    newRange.EndDate > cRange.StartDate
                )
            )
        );
    }
}
