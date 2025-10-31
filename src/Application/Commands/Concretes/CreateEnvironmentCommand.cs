using AutoMapper;
using EnvironmentsService.Src.Application.Commands.Interfaces;
using EnvironmentsService.Src.Application.DTOs.Create;
using EnvironmentsService.Src.Application.DTOs.Get;
using EnvironmentsService.Src.Application.Interfaces;
using EnvironmentsService.Src.Domain.Entities;
using EnvironmentsService.Src.Domain.Interfaces;

namespace EnvironmentsService.Src.Application.Commands.Concretes;

public class CreateEnvironmentCommand(
    CreateEnvironmentDto dto,
    Guid userId,
    IEnvironmentRepository repository,
    IAreaRepository areaRepository,
    IServiceRepository serviceRepository,
    ITypeRepository typeRepository,
    IMapper mapper,
    IImageStorageServiceAdapter imageStorageService,
    IAdminServiceAdapter adminServiceAdapter) : ICommand<EnvironmentDto>
{
    private readonly CreateEnvironmentDto _dto = dto;
    private readonly Guid _userId = userId;
    private readonly IEnvironmentRepository _repository = repository;
    private readonly IAreaRepository _areaRepository = areaRepository;
    private readonly IServiceRepository _serviceRepository = serviceRepository;
    private readonly ITypeRepository _typeRepository = typeRepository;
    private readonly IMapper _mapper = mapper;
    private readonly IImageStorageServiceAdapter _imageStorageService = imageStorageService;
    private readonly IAdminServiceAdapter _adminServiceAdapter = adminServiceAdapter;

    public async Task<EnvironmentDto> ExecuteAsync()
    {
        var environment = _mapper.Map<Domain.Entities.Environment>(_dto);
        environment.Id = Guid.NewGuid();
        environment.OwnerId = _userId;

        var typeId = await _typeRepository.GetIdByPublicKeyAsync(_dto.TypePublicKey);
        environment.TypeId = typeId;

        var serviceMap = await _serviceRepository.GetIdsByPublicKeysAsync(_dto.ServicePublicKeys);
        environment.EnvironmentServices = [.. serviceMap.Values.Select(id => new EnvironmentService
        {
            ServiceId = id,
        })];

        if (_dto.Areas != null)
        {
            var areaKeys = _dto.Areas.Select(a => a.AreaPublicKey).ToList();
            var areaMap = await _areaRepository.GetIdsByPublicKeysAsync(areaKeys);
            environment.EnvironmentAreas = [.. _dto.Areas.Select(a => new EnvironmentArea
            {
                AreaId = areaMap[a.AreaPublicKey],
                Quantity = a.Quantity,
            })];
        }

        if (environment.PricingPolicies != null)
        {
            foreach (var policy in environment.PricingPolicies)
            {
                policy.Environment = environment;
                var area = _mapper.Map<Domain.Entities.Environment>(_dto);
            }
        }

        if (environment.DiscountPolicies != null)
        {
            foreach (var discount in environment.DiscountPolicies)
            {
                discount.Environment = environment;
            }
        }

        if (environment.WeeklySchedules != null)
        {
            foreach (var schedule in environment.WeeklySchedules)
            {
                schedule.Environment = environment;
            }
        }

        if (!string.IsNullOrWhiteSpace(_dto.EquipmentJson))
        {
            environment.Equipment = _dto.EquipmentJson;
        }

        await _repository.AddAsync(environment);
        await _repository.SaveChangesAsync();

        if (_dto.Images?.Count > 0)
        {
            var uploadedFiles = await _imageStorageService.UploadImagesAsync(_dto.Images, "spacio", "environments");

            foreach (var uploaded in uploadedFiles)
            {
                await _repository.AddImageAsync(new EnvironmentPhoto
                {
                    EnvironmentId = environment.Id,
                    Url = uploaded.FileUrl,
                    FileName = uploaded.FileName,
                    FileId = uploaded.FileId,
                });
            }
        }

        if (_dto.Request360Tour)
        {
            await _adminServiceAdapter.RequestTourAsync(environment.PublicId, _userId);
        }

        return _mapper.Map<EnvironmentDto>(environment);
    }
}
