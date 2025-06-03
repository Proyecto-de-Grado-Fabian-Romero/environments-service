using EnvironmentsService.src.Application.DTOs.Create;
using EnvironmentsService.Src.Application.DTOs.Get;
using EnvironmentsService.Src.Application.Interfaces;
using EnvironmentsService.Src.Domain.Entities;
using EnvironmentsService.Src.Domain.Interfaces;

namespace EnvironmentsService.Src.Application.Services;

public class AvailabilityService(
    IEnvironmentRepository environmentRepo,
    IAvailabilityRepository availabilityRepo) : IAvailabilityService
{
    private readonly IEnvironmentRepository _envRepo = environmentRepo;
    private readonly IAvailabilityRepository _availRepo = availabilityRepo;

    public async Task<List<TimeRange>> GetUnavailableTimeSlotsAsync(Guid envId, long startTimestamp, long endTimestamp)
    {
        var start = DateTimeOffset.FromUnixTimeMilliseconds(startTimestamp).UtcDateTime;
        var end = DateTimeOffset.FromUnixTimeMilliseconds(endTimestamp).UtcDateTime;

        var nonAvailabilities = await _availRepo.GetNonAvailabilitiesAsync(envId, start, end);
        var reservations = await _availRepo.GetReservationsAsync(envId, start, end);

        var allRanges = new List<TimeRange>();

        allRanges.AddRange(nonAvailabilities.Select(n =>
            new TimeRange(n.StartDate, n.EndDate)));

        allRanges.AddRange(reservations
            .SelectMany(r => r.TimeRanges)
            .Select(tr => new TimeRange(tr.StartDate, tr.EndDate)));

        return allRanges;
    }

    public async Task BlockDateAsync(BlockAvailabilityRequest request, Guid ownerId)
    {
        var environment = await _envRepo.GetSingleEnvironment(request.EnvironmentId)
            ?? throw new Exception("El ambiente no existe");

        if (environment.OwnerId != ownerId)
        {
            throw new Exception("No tienes permiso para modificar este ambiente");
        }

        var date = DateTimeOffset.FromUnixTimeMilliseconds(request.Date).UtcDateTime.Date;

        var start = new DateTimeOffset(date).ToUnixTimeMilliseconds();
        var end = new DateTimeOffset(date.AddDays(1).AddMinutes(-1)).ToUnixTimeMilliseconds(); // 23:59

        await _availRepo.AddAsync(new NonAvailability
        {
            EnvironmentId = request.EnvironmentId,
            StartDate = start,
            EndDate = end,
        });

        await _availRepo.SaveChangesAsync();
    }
}
