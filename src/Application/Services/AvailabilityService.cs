using EnvironmentsService.Src.Application.DTOs.Get;
using EnvironmentsService.Src.Application.Interfaces;
using EnvironmentsService.Src.Domain.Interfaces;

namespace EnvironmentsService.Src.Application.Services;

public class AvailabilityService(IAvailabilityRepository availabilityRepo) : IAvailabilityService
{
    private readonly IAvailabilityRepository _availabilityRepo = availabilityRepo;

    public async Task<List<TimeRange>> GetUnavailableTimeSlotsAsync(Guid envId, long startTimestamp, long endTimestamp)
    {
        var start = DateTimeOffset.FromUnixTimeSeconds(startTimestamp).UtcDateTime;
        var end = DateTimeOffset.FromUnixTimeSeconds(endTimestamp).UtcDateTime;

        var nonAvailabilities = await _availabilityRepo.GetNonAvailabilitiesAsync(envId, start, end);
        var reservations = await _availabilityRepo.GetReservationsAsync(envId, start, end);

        var allRanges = new List<TimeRange>();

        allRanges.AddRange(nonAvailabilities.Select(n =>
            new TimeRange(n.StartDate, n.EndDate)));

        allRanges.AddRange(reservations.Select(r =>
            new TimeRange(r.StartDate, r.EndDate)));

        return allRanges;
    }
}
