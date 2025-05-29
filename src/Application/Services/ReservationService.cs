namespace EnvironmentsService.Src.Application.Services;

using AutoMapper;
using EnvironmentsService.Src.Application.Commands.Concretes;
using EnvironmentsService.Src.Application.DTOs.Create;
using EnvironmentsService.Src.Application.DTOs.Responses;
using EnvironmentsService.Src.Application.Interfaces;
using EnvironmentsService.Src.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

public class ReservationService(
    IEnvironmentRepository environmentRepo,
    IReservationRepository reservationRepo,
    IMapper mapper) : IReservationService
{
    private readonly IEnvironmentRepository _envRepo = environmentRepo;
    private readonly IReservationRepository _resRepo = reservationRepo;
    private readonly IMapper _mapper = mapper;

    public async Task<ReservationResponse> CreateAsync(CreateReservationRequest request, Guid renterId)
    {
        var dto = _mapper.Map<CreateReservationDto>(request);
        dto.RenterId = renterId;
        var command = new CreateReservationCommand(dto, _envRepo, _resRepo, _mapper);
        return await command.ExecuteAsync();
    }

    public async Task<List<ReservationResponse>> GetByUserAsync(Guid userId, string? status, int page, int limit)
    {
        var query = _resRepo.Query()
            .Where(r => r.RenterId == userId)
            .Include(r => r.Environment)
            .ThenInclude(e => e.Photos)
            .OrderByDescending(r => r.CreatedAt);

        var validStatuses = new[] { "pending", "confirmed", "rejected", "cancelled", "paid" };
        var normalizedStatus = status?.ToLower();

        if (!validStatuses.Contains(normalizedStatus))
        {
            normalizedStatus = "confirmed";
        }

        query = (IOrderedQueryable<Domain.Entities.Booking.Reservation>)query.Where(r => r.Status.Equals(normalizedStatus, StringComparison.CurrentCultureIgnoreCase));

        var reservations = await query
            .Skip((page - 1) * limit)
            .Take(limit)
            .ToListAsync();

        return _mapper.Map<List<ReservationResponse>>(reservations);
    }
}
