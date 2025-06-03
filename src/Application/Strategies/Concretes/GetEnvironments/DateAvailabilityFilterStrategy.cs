using EnvironmentsService.Src.Application.DTOs.GetRequest;
using EnvironmentsService.Src.Application.Strategies.Interfaces;

namespace EnvironmentsService.Src.Application.Strategies.Concretes.GetEnvironments;

public class DateAvailabilityFilterStrategy : IEnvironmentFilterStrategy
{
    public IQueryable<Domain.Entities.Environment> Apply(
        IQueryable<Domain.Entities.Environment> query,
        GetAvailableEnvironmentsRequest request)
    {
        if (request.StartDate.HasValue && request.EndDate.HasValue)
        {
            var start = DateTimeOffset.FromUnixTimeMilliseconds(request.StartDate.Value).UtcDateTime;
            var end = DateTimeOffset.FromUnixTimeMilliseconds(request.EndDate.Value).UtcDateTime;

            query = query.Where(e =>
                !e.NonAvailabilities.Any(n =>
                    n.StartDate < request.EndDate && n.EndDate > request.StartDate));

            var dates = Enumerable.Range(0, (end.Date - start.Date).Days + 1)
                .Select(offset => start.Date.AddDays(offset))
                .ToList();

            foreach (var date in dates)
            {
                var dow = (int)date.DayOfWeek;
                var startMinutes = date == start.Date ? (start.Hour * 60) + start.Minute : 0;
                var endMinutes = date == end.Date ? (end.Hour * 60) + end.Minute : 1440;

                query = query.Where(e =>
                    e.WeeklySchedules.Any(ws =>
                        ws.DayOfWeek == dow &&
                        ws.StartTime <= startMinutes &&
                        ws.EndTime >= endMinutes));
            }
        }

        return query;
    }
}
