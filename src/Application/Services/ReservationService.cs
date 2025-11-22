namespace EnvironmentsService.Src.Application.Services;

using AutoMapper;
using EnvironmentsService.Src.Application.Commands.Concretes;
using EnvironmentsService.Src.Application.DTOs.Create;
using EnvironmentsService.Src.Application.DTOs.Get;
using EnvironmentsService.Src.Application.DTOs.Responses;
using EnvironmentsService.Src.Application.Interfaces;
using EnvironmentsService.Src.Application.Ports;
using EnvironmentsService.Src.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

public class ReservationService(
    IEnvironmentRepository environmentRepo,
    IReservationRepository reservationRepo,
    IAdminServiceAdapter adminServiceAdapter,
    IMapper mapper,
    INotificationsPublisher notificationsPublisher
) : IReservationService
{
    private readonly IEnvironmentRepository _envRepo = environmentRepo;
    private readonly IReservationRepository _resRepo = reservationRepo;
    private readonly IAdminServiceAdapter _adminServiceAdapter = adminServiceAdapter;
    private readonly IMapper _mapper = mapper;
    private readonly INotificationsPublisher _notificationsPublisher = notificationsPublisher;

    public async Task<ReservationResponse> CreateAsync(
        CreateReservationRequest request,
        Guid renterId
    )
    {
        var dto = _mapper.Map<CreateReservationDto>(request);
        dto.RenterId = renterId;
        var command = new CreateReservationCommand(
            dto,
            _envRepo,
            _resRepo,
            _mapper,
            _notificationsPublisher
        );
        return await command.ExecuteAsync();
    }

    public async Task<PagedResult<ReservationResponse>> GetByUserAsync(
        Guid userId,
        string? status,
        string? type, // nuevo
        int page,
        int limit
    )
    {
        var validStatuses = new[] { "pending", "confirmed", "rejected", "cancelled", "paid" };
        var normalizedStatus = validStatuses.Contains(status?.ToLower())
            ? status!.ToLower()
            : (status == null ? null : "confirmed");

        var normalizedType = type?.ToLower();
        if (normalizedType == "owner")
        {
            normalizedType = "mine";
        }

        if (normalizedType == "renter")
        {
            normalizedType = "others";
        }

        var (reservations, totalItems) = await _resRepo.GetUserReservationsPaginatedAsync(
            userId,
            normalizedStatus,
            normalizedType,
            page,
            limit
        );

        return new PagedResult<ReservationResponse>
        {
            Items = _mapper.Map<List<ReservationResponse>>(reservations),
            CurrentPage = page,
            Limit = limit,
            TotalItems = totalItems,
            TotalPages = (int)Math.Ceiling((double)totalItems / limit),
        };
    }

    public async Task<ReservationResponse?> GetByPublicIdAsync(Guid publicId)
    {
        var reservation = await _resRepo.GetByPublicIdAsync(publicId);
        return reservation == null ? null : _mapper.Map<ReservationResponse>(reservation);
    }

    public async Task<ReservationResponse> UpdateStatusAsync(
        Guid reservationPublicId,
        string newStatus
    )
    {
        var command = new UpdateReservationStatusCommand(
            reservationPublicId,
            newStatus,
            _resRepo,
            _adminServiceAdapter,
            _mapper,
            _notificationsPublisher
        );
        return await command.ExecuteAsync();
    }

    public async Task<List<ReservationResponse>> GetConflictingReservationsAsync(
        Guid environmentId,
        long start,
        long end
    )
    {
        var conflicts = await _resRepo.GetConflictsAsync(environmentId, start, end);
        return _mapper.Map<List<ReservationResponse>>(conflicts);
    }

    public async Task<PagedResult<ReservationResponse>> GetByUserAndDayAsync(
        Guid userId,
        long scheduledDayTimestamp,
        string? status,
        string? type,
        int page,
        int limit
    )
    {
        var validStatuses = new[] { "pending", "confirmed", "rejected", "cancelled", "paid" };
        var normalizedStatus = validStatuses.Contains(status?.ToLower())
            ? status!.ToLower()
            : (status == null ? null : "confirmed");

        var normalizedType = type?.ToLower();
        if (normalizedType == "owner")
        {
            normalizedType = "mine";
        }

        if (normalizedType == "renter")
        {
            normalizedType = "others";
        }

        var (reservations, totalItems) = await _resRepo.GetUserReservationsByDayAsync(
            userId,
            scheduledDayTimestamp,
            normalizedStatus,
            normalizedType,
            page,
            limit
        );

        return new PagedResult<ReservationResponse>
        {
            Items = _mapper.Map<List<ReservationResponse>>(reservations),
            CurrentPage = page,
            Limit = limit,
            TotalItems = totalItems,
            TotalPages = (int)Math.Ceiling((double)totalItems / limit),
        };
    }
}
