using AutoMapper;
using EnvironmentsService.Src.Application.Commands.Concretes;
using EnvironmentsService.Src.Application.DTOs.Create;
using EnvironmentsService.Src.Application.DTOs.Get;
using EnvironmentsService.Src.Application.DTOs.GetRequest;
using EnvironmentsService.Src.Application.DTOs.Update;
using EnvironmentsService.Src.Application.Interfaces;
using EnvironmentsService.Src.Domain.Interfaces;

namespace EnvironmentsService.Src.Application.Services;

public class EnvironmentService(
    IEnvironmentRepository repository,
    IAreaRepository areaRepository,
    IServiceRepository serviceRepository,
    ITypeRepository typeRepository,
    IImageStorageServiceAdapter imageStorageService,
    IAdminServiceAdapter adminServiceAdapter,
    IMapper mapper
) : IEnvironmentService
{
    private readonly IEnvironmentRepository _repository = repository;
    private readonly IAreaRepository _areaRepository = areaRepository;
    private readonly IServiceRepository _serviceRepository = serviceRepository;
    private readonly ITypeRepository _typeRepository = typeRepository;
    private readonly IMapper _mapper = mapper;
    private readonly IImageStorageServiceAdapter _imageStorageService = imageStorageService;
    private readonly IAdminServiceAdapter _adminServiceAdapter = adminServiceAdapter;

    public async Task<PagedResult<GetAllEnvironmentDto>> GetAvailableEnvironmentsAsync(
        GetAvailableEnvironmentsRequest request,
        int page,
        int limit,
        Guid? userPublicId
    )
    {
        var command = new GetAllEnvironmentsCommand(
            _repository,
            _mapper,
            request,
            page,
            limit,
            userPublicId
        );
        return await command.ExecuteAsync();
    }

    public async Task<PagedResult<GetAllEnvironmentDto>> GetOwnerEnvironmentsAsync(
        Guid publicUserId,
        int page,
        int limit
    )
    {
        var command = new GetOwnerEnvironments(_repository, _mapper, publicUserId, page, limit);
        return await command.ExecuteAsync();
    }

    public async Task<EnvironmentDto?> GetSingleEnvironmentAsync(Guid publicId)
    {
        var command = new GetSingleEnvironmentCommand(_repository, _mapper, publicId);
        return await command.ExecuteAsync();
    }

    public async Task<EnvironmentDto> CreateAsync(CreateEnvironmentDto dto, Guid userId)
    {
        var command = new CreateEnvironmentCommand(
            dto,
            userId,
            _repository,
            _areaRepository,
            _serviceRepository,
            _typeRepository,
            _mapper,
            _imageStorageService,
            _adminServiceAdapter
        );

        return await command.ExecuteAsync();
    }

    public async Task UpdateDetectedObjectsAsync(
        Guid publicId,
        Dictionary<string, int> detectedObjects
    )
    {
        var command = new UpdateDetectedObjectsCommand(_repository, publicId, detectedObjects);
        await command.ExecuteAsync();
    }

    public async Task<List<AvailableEquipmentDto>> GetAvailableEquipmentAsync(
        GetAvailableEnvironmentsRequest request
    )
    {
        var command = new GetAvailableEquipmentCommand(request, _repository);
        return await command.ExecuteAsync();
    }

    public async Task<EnvironmentDto> UpdateAsync(
        Guid publicId,
        UpdateEnvironmentDto dto,
        Guid userId
    )
    {
        var command = new UpdateEnvironmentCommand(
            publicId,
            userId,
            dto,
            _repository,
            _areaRepository,
            _serviceRepository,
            _typeRepository,
            _mapper,
            _imageStorageService,
            _adminServiceAdapter
        );

        var environment = await command.ExecuteAsync();
        return _mapper.Map<EnvironmentDto>(environment);
    }

    public async Task<bool> PatchHideEnvironmentAsync(
        Guid environmentPublicId,
        bool hide,
        Guid userPublicId
    )
    {
        return await _repository.SetHiddenByPublicIdAsync(environmentPublicId, hide, userPublicId);
    }

    public async Task<bool> PatchDeleteEnvironmentAsync(Guid environmentPublicId, Guid userPublicId)
    {
        return await _repository.SoftDeleteByPublicIdAsync(environmentPublicId, userPublicId);
    }
}
