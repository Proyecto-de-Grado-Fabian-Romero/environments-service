using EnvironmentsService.Src.Domain.Entities.Booking;
using Microsoft.EntityFrameworkCore.Storage;

namespace EnvironmentsService.Src.Domain.Interfaces;

public interface IReservationRepository
{
    Task AddAsync(Reservation reservation);

    Task<IDbContextTransaction> BeginTransactionAsync();

    Task<bool> ExistsOverlappingReservationAsync(Guid environmentId, long start, long end);

    Task SaveChangesAsync();
}
