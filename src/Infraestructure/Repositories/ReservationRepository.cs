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

    public async Task<List<Reservation>> GetByUserAsync(Guid userPublicId)
    {
        return await _context.Set<Reservation>()
            .Include(r => r.Environment)
            .Where(r =>
                r.RenterId == userPublicId ||
                r.OwnerId == userPublicId)
            .ToListAsync();
    }

    public IQueryable<Reservation> Query()
    {
        return _context.Set<Reservation>().AsQueryable();
    }
}
