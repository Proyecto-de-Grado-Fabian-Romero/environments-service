using EnvironmentsService.Src.Domain.Entities.Booking;
using Microsoft.EntityFrameworkCore.Storage;

namespace EnvironmentsService.Src.Domain.Interfaces;

public interface IReservationRepository
{
    Task AddAsync(Reservation reservation);

    Task<IDbContextTransaction> BeginTransactionAsync();

    Task<bool> ExistsOverlappingReservationAsync(Guid environmentId, long start, long end);

    Task<List<Reservation>> GetActiveReservationsByRenterAsync(Guid renterId);

    Task SaveChangesAsync();

    Task<(List<Reservation>, int)> GetUserReservationsPaginatedAsync(Guid userId, string status, int page, int limit);

    Task<Reservation?> GetByPublicIdAsync(Guid publicId);

    Task<bool> ExistsOverlappingConfirmedAsync(
        Guid environmentId,
        Guid currentReservationId,
        ICollection<ReservationTimeRange> timeRanges);

    Task<List<Reservation>> GetConflictsAsync(Guid environmentId, long start, long end);

    Task<List<Reservation>> GetByOwnerAndDayAsync(Guid ownerId, long timestamp);

    Task AddPaymentAsync(Guid reservationId, ReservationPayment payment);

    Task MarkPaymentAsPaidAsync(Guid reservationId, string method, long paidAt);
}
