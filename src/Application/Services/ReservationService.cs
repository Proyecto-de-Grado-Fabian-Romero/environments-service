namespace EnvironmentsService.Src.Application.Services;

using AutoMapper;
using EnvironmentsService.Src.Application.Commands.Concretes;
using EnvironmentsService.Src.Application.DTOs.Create;
using EnvironmentsService.Src.Application.DTOs.Responses;
using EnvironmentsService.Src.Application.Interfaces;
using EnvironmentsService.Src.Domain.Interfaces;

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
}
