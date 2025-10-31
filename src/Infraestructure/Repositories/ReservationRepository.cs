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

    public async Task<bool> ExistsOverlappingReservationAsync(
        Guid environmentId,
        long start,
        long end,
        bool isInstant
    )
    {
        if (start >= end)
        {
            throw new ArgumentException("start debe ser menor que end");
        }

        var boliviaOffset = TimeSpan.FromHours(-4);

        var nowUtc = DateTimeOffset.UtcNow;
        var nowBolivia = nowUtc.ToOffset(boliviaOffset);
        long nowUnixSeconds = nowBolivia.ToUnixTimeSeconds();

        long cutoffInstant = nowUnixSeconds - (15 * 60); // 15 minutos
        long cutoffNonInstant = nowUnixSeconds - (12 * 3600); // 12 horas

        var reservationsToRejectQuery = _context
            .Set<Reservation>()
            .Where(r =>
                r.EnvironmentId == environmentId
                && r.Status.ToLower() == "confirmed"
                && (
                    (isInstant && (r.ConfirmedAt <= cutoffInstant))
                    || (!isInstant && (r.ConfirmedAt <= cutoffNonInstant))
                    || (!isInstant && r.TimeRanges.First().StartDate < nowUnixSeconds)
                )
            );

        var reservationsToReject = await reservationsToRejectQuery.ToListAsync();

        if (reservationsToReject.Any())
        {
            foreach (var rr in reservationsToReject)
            {
                rr.Status = "rejected";
            }

            await _context.SaveChangesAsync();
        }

        var overlap = await _context
            .Set<ReservationTimeRange>()
            .Where(tr =>
                tr.Reservation.EnvironmentId == environmentId
                && tr.Reservation.Status.ToLower() != "cancelled"
                && tr.Reservation.Status.ToLower() != "rejected"
                && start < tr.EndDate
                && end > tr.StartDate
            )
            .AnyAsync();

        return overlap;
    }

    public async Task<List<Reservation>> GetActiveReservationsByRenterAsync(Guid renterId)
    {
        return await _context
            .Set<Reservation>()
            .Include(r => r.Environment)
            .Include(r => r.TimeRanges)
            .Where(r => r.RenterId == renterId && r.Status != "cancelled" && r.Status != "rejected")
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();
    }

    public async Task<Reservation?> GetByPublicIdAsync(Guid publicId)
    {
        return await _context
            .Set<Reservation>()
            .Include(r => r.Environment)
            .ThenInclude(e => e.Photos)
            .Include(r => r.TimeRanges)
            .FirstOrDefaultAsync(r => r.PublicId == publicId);
    }

    public async Task<Reservation?> GetByIdAsync(Guid id)
    {
        return await _context
            .Set<Reservation>()
            .Include(r => r.Environment)
            .ThenInclude(e => e.Photos)
            .FirstOrDefaultAsync(r => r.Id == id);
    }

    public async Task<bool> ExistsOverlappingConfirmedAsync(
        Guid environmentId,
        Guid currentReservationId,
        ICollection<ReservationTimeRange> timeRanges
    )
    {
        var confirmed = await _context
            .Set<Reservation>()
            .Include(r => r.TimeRanges)
            .Where(r =>
                r.EnvironmentId == environmentId
                && (r.Status == "confirmed" || r.Status == "paid")
                && r.Id != currentReservationId
            )
            .ToListAsync();

        return confirmed.Any(conf =>
            conf.TimeRanges.Any(cRange =>
                timeRanges.Any(newRange =>
                    newRange.StartDate < cRange.EndDate && newRange.EndDate > cRange.StartDate
                )
            )
        );
    }

    public async Task<(List<Reservation>, int)> GetUserReservationsPaginatedAsync(
        Guid userId,
        string? status,
        string? type, // "mine" | "others" | null
        int page,
        int limit
    )
    {
        var query = _context
            .Set<Reservation>()
            .Include(r => r.Environment)
            .ThenInclude(e => e.Photos)
            .Include(r => r.TimeRanges)
            .Include(r => r.Payments)
            .AsQueryable();

        if (!string.IsNullOrEmpty(type))
        {
            if (type == "mine")
            {
                query = query.Where(r => r.OwnerId == userId);
            }
            else
            {
                query = query.Where(r => r.RenterId == userId);
            }
        }
        else
        {
            query = query.Where(r => r.RenterId == userId);
        }

        var timezoneOffset = TimeSpan.FromHours(-4);
        var currentTimeInTimezone = DateTimeOffset
            .UtcNow.ToOffset(new TimeSpan(-4, 0, 0))
            .ToOffset(timezoneOffset);
        var sixteenMinutesAgo = currentTimeInTimezone.AddMinutes(-16);
        var timeLimitTimestamp = sixteenMinutesAgo.ToUnixTimeSeconds();

        query = query.Where(r =>
            !r.IsInstant
            || (
                r.IsInstant
                && (
                    r.Status.ToLower() == "paid"
                    || (r.Status.ToLower() == "confirmed" && r.CreatedAt > timeLimitTimestamp)
                )
            )
        );

        var totalItems = await query.CountAsync();

        if (totalItems == 0)
        {
            return (new List<Reservation>(), 0);
        }

        var now = DateTimeOffset.UtcNow.ToOffset(new TimeSpan(-4, 0, 0)).ToUnixTimeSeconds();

        var reservations = await query
            .OrderBy(r => r.TimeRanges.Min(tr => tr.StartDate) < now)
            .ThenBy(r => r.TimeRanges.Min(tr => tr.StartDate))
            .Skip((page - 1) * limit)
            .Take(limit)
            .ToListAsync();

        return (reservations, totalItems);
    }

    public async Task<List<Reservation>> GetConflictsAsync(Guid environmentId, long start, long end)
    {
        return await _context
            .Set<Reservation>()
            .Where(r => r.EnvironmentId == environmentId)
            .Where(r => r.Status == "pending" || r.Status == "confirmed")
            .Where(r => r.TimeRanges.Any(tr => start < tr.EndDate && end > tr.StartDate))
            .Include(r => r.Environment)
            .Include(r => r.TimeRanges)
            .ToListAsync();
    }

    public async Task AddPaymentAsync(Guid reservationId, ReservationPayment payment)
    {
        var reservation =
            await _context
                .Set<Reservation>()
                .Include(r => r.Payments)
                .FirstOrDefaultAsync(r => r.PublicId == reservationId)
            ?? throw new Exception("Reservation not found");
        reservation.Payments.Add(payment);
    }

    public async Task MarkPaymentAsPaidAsync(Guid reservationId, string method, long paidAt)
    {
        var reservation =
            await _context
                .Set<Reservation>()
                .Include(r => r.Payments)
                .FirstOrDefaultAsync(r => r.PublicId == reservationId)
            ?? throw new Exception("Reservation not found");
        var payment =
            reservation.Payments.FirstOrDefault(p => p.Method == method && p.Status == "pending")
            ?? throw new Exception("No pending payment found for method: " + method);
        payment.Status = "paid";
        payment.PaidAt = paidAt;
    }

    public async Task<(List<Reservation>, int)> GetUserReservationsByDayAsync(
    Guid userId,
    long scheduledDayTimestamp,
    string? status,
    string? type,
    int page,
    int limit
)
    {
        var query = _context
            .Set<Reservation>()
            .Include(r => r.Environment)
            .ThenInclude(e => e.Photos)
            .Include(r => r.TimeRanges)
            .Include(r => r.Payments)
            .AsSplitQuery()
            .AsQueryable();

        if (!string.IsNullOrEmpty(type))
        {
            if (type == "mine")
            {
                Console.WriteLine("Filtering by owner");
                query = query.Where(r => r.OwnerId == userId);
            }
            else
            {
                Console.WriteLine("Filtering by renter");
                query = query.Where(r => r.RenterId == userId);
            }
        }
        else
        {
            query = query.Where(r => r.RenterId == userId);
        }

        var scheduledDay = DateTimeOffset
            .FromUnixTimeSeconds(scheduledDayTimestamp)
            .ToOffset(TimeSpan.FromHours(-4))
            .Date;

        Console.WriteLine($"=== FILTERING BY DAY ===");
        Console.WriteLine($"Scheduled Day: {scheduledDay:yyyy-MM-dd}");
        Console.WriteLine($"Scheduled Day Timestamp: {scheduledDayTimestamp}");

        var startOfDayTimestamp = new DateTimeOffset(scheduledDay).ToUnixTimeSeconds();
        var endOfDayTimestamp = new DateTimeOffset(scheduledDay.AddDays(1)).ToUnixTimeSeconds();

        Console.WriteLine($"Start of Day Timestamp: {startOfDayTimestamp}");
        Console.WriteLine($"End of Day Timestamp: {endOfDayTimestamp}");
        Console.WriteLine($"Start of Day: {DateTimeOffset.FromUnixTimeSeconds(startOfDayTimestamp):yyyy-MM-dd HH:mm:ss}");
        Console.WriteLine($"End of Day: {DateTimeOffset.FromUnixTimeSeconds(endOfDayTimestamp):yyyy-MM-dd HH:mm:ss}");

        // Log todas las reservas antes del filtro por día
        var allReservationsBeforeDayFilter = await query.ToListAsync();
        Console.WriteLine($"=== RESERVATIONS BEFORE DAY FILTER: {allReservationsBeforeDayFilter.Count} ===");

        foreach (var reservation in allReservationsBeforeDayFilter)
        {
            Console.WriteLine($"Reservation ID: {reservation.PublicId}, Status: {reservation.Status}, IsInstant: {reservation.IsInstant}");
            Console.WriteLine($"TimeRanges Count: {reservation.TimeRanges?.Count ?? 0}");

            if (reservation.TimeRanges != null && reservation.TimeRanges.Any())
            {
                foreach (var timeRange in reservation.TimeRanges)
                {
                    var isInRange = timeRange.StartDate >= startOfDayTimestamp && timeRange.StartDate < endOfDayTimestamp;

                    Console.WriteLine($"  - TimeRange: {timeRange.StartDate} to {timeRange.EndDate}");
                    Console.WriteLine($"    Is in target day range: {isInRange}");
                }
            }
            else
            {
                Console.WriteLine("  - NO TIMERANGES");
            }
            Console.WriteLine("---");
        }

        query = query.Where(r =>
            r.TimeRanges.Any(tr =>
                tr.StartDate >= startOfDayTimestamp && tr.StartDate < endOfDayTimestamp
            )
        );

        var reservationsAfterDayFilter = await query.ToListAsync();
        Console.WriteLine($"=== RESERVATIONS AFTER DAY FILTER: {reservationsAfterDayFilter.Count} ===");

        var timezoneOffset = TimeSpan.FromHours(-4);
        var currentTimeInTimezone = DateTimeOffset.UtcNow.ToOffset(timezoneOffset);
        var sixteenMinutesAgo = currentTimeInTimezone.AddMinutes(-16);
        var timeLimitTimestamp = sixteenMinutesAgo.ToUnixTimeSeconds();

        Console.WriteLine($"=== INSTANT BOOKING FILTER ===");
        Console.WriteLine($"Current Time (-4): {currentTimeInTimezone:yyyy-MM-dd HH:mm:ss}");
        Console.WriteLine($"16 Minutes Ago: {sixteenMinutesAgo:yyyy-MM-dd HH:mm:ss}");
        Console.WriteLine($"Time Limit Timestamp: {timeLimitTimestamp}");

        query = query.Where(r =>
            !r.IsInstant
            || (
                r.IsInstant
                && (
                    r.Status.ToLower() == "paid"
                    || (r.Status.ToLower() == "confirmed" && r.CreatedAt > timeLimitTimestamp)
                )
            )
        );

        var reservationsAfterAllFilters = await query.ToListAsync();
        Console.WriteLine($"=== RESERVATIONS AFTER ALL FILTERS: {reservationsAfterAllFilters.Count} ===");

        foreach (var reservation in reservationsAfterAllFilters)
        {
            Console.WriteLine($"Reservation ID: {reservation.PublicId}, Status: {reservation.Status}, IsInstant: {reservation.IsInstant}, CreatedAt: {reservation.CreatedAt}");
            if (reservation.IsInstant && reservation.Status.ToLower() == "confirmed")
            {
                var createdAt = DateTimeOffset.FromUnixTimeSeconds(reservation.CreatedAt);
                var canShow = reservation.CreatedAt > timeLimitTimestamp;
                Console.WriteLine($"  - Instant Confirmed Check: CreatedAt={createdAt:yyyy-MM-dd HH:mm:ss}, CanShow={canShow}");
            }
        }

        var totalItems = await query.CountAsync();
        Console.WriteLine($"=== FINAL TOTAL ITEMS: {totalItems} ===");

        if (totalItems == 0)
        {
            Console.WriteLine("=== NO RESERVATIONS FOUND ===");
            return (new List<Reservation>(), 0);
        }

        var now = DateTimeOffset.UtcNow.ToOffset(TimeSpan.FromHours(-4)).ToUnixTimeSeconds();
        Console.WriteLine($"Now Timestamp (ms): {now}");

        var reservations = await query
            .OrderBy(r => r.TimeRanges.Min(tr => tr.StartDate) < now)
            .ThenBy(r => r.TimeRanges.Min(tr => tr.StartDate))
            .Skip((page - 1) * limit)
            .Take(limit)
            .ToListAsync();

        Console.WriteLine($"=== FINAL RESERVATIONS TO RETURN: {reservations.Count} ===");
        foreach (var reservation in reservations)
        {
            var minStartDate = reservation.TimeRanges.Min(tr => tr.StartDate);
            var minStartDateTime = DateTimeOffset.FromUnixTimeSeconds(minStartDate);
            var isPast = minStartDate < now / 1000; // Convertir now a segundos para comparar

            Console.WriteLine($"Reservation ID: {reservation.PublicId}");
            Console.WriteLine($"  - Min Start Date: {minStartDateTime:yyyy-MM-dd HH:mm:ss} (Timestamp: {minStartDate})");
            Console.WriteLine($"  - Is Past: {isPast}");
            Console.WriteLine($"  - Status: {reservation.Status}, IsInstant: {reservation.IsInstant}");
        }

        return (reservations, totalItems);
    }
}
