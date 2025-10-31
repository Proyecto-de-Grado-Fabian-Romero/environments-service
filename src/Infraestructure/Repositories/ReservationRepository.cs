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
        return false;

        // return await _context.Set<Reservation>()
        //     .Include(r => r.TimeRanges)
        //     .Where(r => r.EnvironmentId == environmentId &&
        //                 r.Status != "cancelled" &&
        //                 r.Status != "rejected")
        //     .AnyAsync(r =>
        //         r.TimeRanges.Any(tr =>
        //             start < tr.EndDate && end > tr.StartDate));
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
                    newRange.EndDate > cRange.StartDate)));
    }

    public async Task<(List<Reservation>, int)> GetUserReservationsPaginatedAsync(Guid userId, string status, int page, int limit)
    {
        var query = _context.Set<Reservation>()
            .Include(r => r.Environment)
                .ThenInclude(e => e.Photos)
            .Include(r => r.TimeRanges)
            .Include(r => r.Payments)
            .Where(r => r.RenterId == userId || r.OwnerId == userId /* && r.Status == status */);

        var totalItems = await query.CountAsync();
        var reservations = await query
            .OrderByDescending(r => r.CreatedAt)
            .Skip((page - 1) * limit)
            .Take(limit)
            .ToListAsync();

        return (reservations, totalItems);
    }

    public async Task<List<Reservation>> GetConflictsAsync(Guid environmentId, long start, long end)
    {
        return await _context.Set<Reservation>()
            .Where(r => r.EnvironmentId == environmentId)
            .Where(r => r.Status == "pending" || r.Status == "confirmed")
            .Where(r =>
                r.TimeRanges.Any(tr =>
                    start < tr.EndDate &&
                    end > tr.StartDate))
            .Include(r => r.Environment)
            .Include(r => r.TimeRanges)
            .ToListAsync();
    }

    public async Task<List<Reservation>> GetByOwnerAndDayAsync(Guid ownerId, long timestamp)
    {
        var startOfDay = DateTimeOffset.FromUnixTimeMilliseconds(timestamp).UtcDateTime.Date;
        var endOfDay = startOfDay.AddDays(1).AddTicks(-1);
        var startUnix = new DateTimeOffset(startOfDay).ToUnixTimeMilliseconds();
        var endUnix = new DateTimeOffset(endOfDay).ToUnixTimeMilliseconds();

        var excludedStatuses = new[] { "rejected", "cancelled" };

        return await _context.Set<Reservation>()
            .Where(r => r.OwnerId == ownerId && !excludedStatuses.Contains(r.Status))
            .Where(r => r.TimeRanges.Any(tr => tr.StartDate < endUnix && tr.EndDate > startUnix))
            .Include(r => r.Environment)
            .ThenInclude(e => e.Photos)
            .ToListAsync();
    }

    public async Task AddPaymentAsync(Guid reservationId, ReservationPayment payment)
    {
        var reservation = await _context.Set<Reservation>()
            .Include(r => r.Payments)
            .FirstOrDefaultAsync(r => r.PublicId == reservationId) ?? throw new Exception("Reservation not found");
        reservation.Payments.Add(payment);
    }

    public async Task MarkPaymentAsPaidAsync(Guid reservationId, string method, long paidAt)
    {
        var reservation = await _context.Set<Reservation>()
            .Include(r => r.Payments)
            .FirstOrDefaultAsync(r => r.PublicId == reservationId) ?? throw new Exception("Reservation not found");
        var payment = reservation.Payments.FirstOrDefault(p => p.Method == method && p.Status == "pending") ?? throw new Exception("No pending payment found for method: " + method);
        payment.Status = "paid";
        payment.PaidAt = paidAt;
    }
}
