using EnvironmentsService.Src.Application.DTOs.GetRequest;
using EnvironmentsService.Src.Application.Strategies.Interfaces;

namespace EnvironmentsService.Src.Application.Strategies.Concretes.GetEnvironments;

public class InstantBookingFilterStrategy : IEnvironmentFilterStrategy
{
    public IQueryable<Domain.Entities.Environment> Apply(
            IQueryable<Domain.Entities.Environment> query,
            GetAvailableEnvironmentsRequest request)
    {
        if (request.InstantBookingRequired.HasValue && request.InstantBookingRequired.Value == true)
        {
            query = query.Where(e => e.InstantBooking == request.InstantBookingRequired);
        }

        return query;
    }
}
