using EnvironmentsService.src.Application.DTOs.Create;
using EnvironmentsService.Src.Application.DTOs.Get;
using EnvironmentsService.Src.Application.DTOs.Responses;
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
        var start = DateTimeOffset.FromUnixTimeSeconds(startTimestamp).UtcDateTime;
        var end = DateTimeOffset.FromUnixTimeSeconds(endTimestamp).UtcDateTime;

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

        if (request.StartDate >= request.EndDate)
        {
            throw new Exception("La hora de inicio debe ser menor que la hora de fin");
        }

        await _availRepo.AddAsync(new NonAvailability
        {
            EnvironmentId = request.EnvironmentId,
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            Type = "OwnerBlocked",
        });

        await _availRepo.SaveChangesAsync();
    }

    public async Task<List<OwnerBlockedAvailabilityDto>> GetOwnerBlockedAsync(Guid ownerId)
    {
        var blocked = await _availRepo.GetOwnerBlockedByOwnerIdAsync(ownerId);

        return [.. blocked.Select(n => new OwnerBlockedAvailabilityDto
        {
            EnvironmentId = n.EnvironmentId,
            EnvironmentTitle = n.Environment.Title,
            EnvironmentPhotoUrl = n.Environment.Photos.OrderBy(p => p.Order).FirstOrDefault()?.Url,
            StartDate = n.StartDate,
            EndDate = n.EndDate,
        })];
    }
}
