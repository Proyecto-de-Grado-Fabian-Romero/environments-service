using EnvironmentsService.Src.Domain.Entities.Booking;

namespace EnvironmentsService.Src.Domain.Interfaces;

public interface IReservationRepository
{
    Task AddAsync(Reservation reservation);

    Task SaveChangesAsync();
}
