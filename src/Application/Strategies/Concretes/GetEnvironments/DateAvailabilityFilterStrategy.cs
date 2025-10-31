using EnvironmentsService.Src.Application.DTOs.GetRequest;
using EnvironmentsService.Src.Application.Strategies.Interfaces;

namespace EnvironmentsService.Src.Application.Strategies.Concretes.GetEnvironments;

public class DateAvailabilityFilterStrategy : IEnvironmentFilterStrategy
{
    private const long MillisecondsThreshold = 100_000_000_000;

    private static DateTime UnixToUtcDateTime(long ts)
    {
        if (Math.Abs(ts) > MillisecondsThreshold)
        {
            return DateTimeOffset.FromUnixTimeMilliseconds(ts).UtcDateTime;
        }

        return DateTimeOffset.FromUnixTimeSeconds(ts).UtcDateTime;
    }

    public IQueryable<Domain.Entities.Environment> Apply(
        IQueryable<Domain.Entities.Environment> query,
        GetAvailableEnvironmentsRequest request
    )
    {
        if (!(request?.StartDate.HasValue == true && request.EndDate.HasValue))
        {
            return query;
        }

        var startUtc = UnixToUtcDateTime(request.StartDate.Value);
        var endUtc = UnixToUtcDateTime(request.EndDate.Value);

        var startUtcSeconds = new DateTimeOffset(startUtc).ToUnixTimeSeconds();
        var endUtcSeconds = new DateTimeOffset(endUtc).ToUnixTimeSeconds();
        var startUtcMillis = new DateTimeOffset(startUtc).ToUnixTimeMilliseconds();
        var endUtcMillis = new DateTimeOffset(endUtc).ToUnixTimeMilliseconds();

        query = query.Where(e =>
            !e.NonAvailabilities.Any(n =>
                (n.StartDate < endUtcMillis && n.EndDate > startUtcMillis)
                || (n.StartDate < endUtcSeconds && n.EndDate > startUtcSeconds)
            )
        );

        query = query.Where(e =>
            !e.Reservations.Any(r =>
                (r.Status == "pending" || r.Status == "confirmed")
                && r.TimeRanges.Any(tr =>
                    (tr.StartDate < endUtcMillis && tr.EndDate > startUtcMillis)
                    || (tr.StartDate < endUtcSeconds && tr.EndDate > startUtcSeconds)
                )
            )
        );

        query = query.Where(e => !e.Hidden && !e.Deleted);

        TimeZoneInfo? localTz = null;
        try
        {
            localTz = TimeZoneInfo.FindSystemTimeZoneById("America/La_Paz");
        }
        catch
        {
            localTz = null;
        }

        DateTime startLocal =
            localTz != null ? TimeZoneInfo.ConvertTimeFromUtc(startUtc, localTz) : startUtc;

        DateTime endLocal =
            localTz != null ? TimeZoneInfo.ConvertTimeFromUtc(endUtc, localTz) : endUtc;

        var daysCount = (endLocal.Date - startLocal.Date).Days + 1;
        if (daysCount <= 0)
        {
            return query;
        }

        var dates = Enumerable
            .Range(0, daysCount)
            .Select(offset => startLocal.Date.AddDays(offset))
            .ToList();

        if (request.EnvironmentTypePublicKey == "hospedajes")
        {
            return query;
        }

        foreach (var date in dates)
        {
            var dow = (int)date.DayOfWeek;

            var startMinutes =
                date == startLocal.Date ? (startLocal.Hour * 60) + startLocal.Minute : 0;

            var endMinutes = date == endLocal.Date ? (endLocal.Hour * 60) + endLocal.Minute : 1440;

            query = query.Where(e =>
                e.WeeklySchedules.Any(ws =>
                    ws.DayOfWeek == dow && ws.StartTime <= startMinutes && ws.EndTime >= endMinutes
                )
            );
        }

        return query;
    }
}
