using EnvironmentsService.Src.Domain.Entities;
using EnvironmentsService.Src.Domain.Entities.Booking;

namespace EnvironmentsService.Src.Domain.Interfaces;

public interface IAvailabilityRepository
{
    Task<IEnumerable<NonAvailability>> GetNonAvailabilitiesAsync(Guid environmentId, DateTime start, DateTime end);

    Task<IEnumerable<Reservation>> GetReservationsAsync(Guid environmentId, DateTime start, DateTime end);

    Task<List<NonAvailability>> GetOwnerBlockedByOwnerIdAsync(Guid ownerId);

    Task AddAsync(NonAvailability availability);

    Task SaveChangesAsync();
}
