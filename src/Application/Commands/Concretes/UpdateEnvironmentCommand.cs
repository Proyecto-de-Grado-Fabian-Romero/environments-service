namespace EnvironmentsService.Src.Application.Commands.Concretes;

using AutoMapper;
using EnvironmentsService.Src.Application.Commands.Interfaces;
using EnvironmentsService.Src.Application.DTOs.Update;
using EnvironmentsService.Src.Application.Interfaces;
using EnvironmentsService.Src.Domain.Entities;
using EnvironmentsService.Src.Domain.Interfaces;

public class UpdateEnvironmentCommand(
    Guid publicId,
    Guid userId,
    UpdateEnvironmentDto dto,
    IEnvironmentRepository repository,
    IAreaRepository areaRepository,
    IServiceRepository serviceRepository,
    ITypeRepository typeRepository,
    IMapper mapper,
    IImageStorageServiceAdapter imageStorageService,
    IAdminServiceAdapter adminServiceAdapter
) : ICommand<Domain.Entities.Environment>
{
    private readonly Guid _publicId = publicId;
    private readonly Guid _userId = userId;
    private readonly UpdateEnvironmentDto _dto = dto;
    private readonly IEnvironmentRepository _repository = repository;
    private readonly IAreaRepository _areaRepository = areaRepository;
    private readonly IServiceRepository _serviceRepository = serviceRepository;
    private readonly ITypeRepository _typeRepository = typeRepository;
    private readonly IMapper _mapper = mapper;
    private readonly IImageStorageServiceAdapter _imageStorageService = imageStorageService;
    private readonly IAdminServiceAdapter _adminServiceAdapter = adminServiceAdapter;

    public async Task<Domain.Entities.Environment> ExecuteAsync()
    {
        // 1) Traer agregado con includes (fotos, services, areas, policies, schedules)
        var environment =
            await _repository.GetByPublicIdWithIncludesAsync(_publicId)
            ?? throw new KeyNotFoundException("Environment not found");

        // 2) Autorización (owner)
        if (environment.OwnerId != _userId)
        {
            throw new UnauthorizedAccessException("You are not the owner of this environment.");
        }

        // 3) Scalars (patch si vienen)
        if (_dto.Title is not null)
        {
            environment.Title = _dto.Title;
        }

        if (_dto.Description is not null)
        {
            environment.Description = _dto.Description;
        }

        if (_dto.Location is not null)
        {
            environment.Location = _dto.Location;
        }

        if (_dto.Latitude.HasValue)
        {
            environment.Latitude = (decimal)_dto.Latitude.Value;
        }

        if (_dto.Longitude.HasValue)
        {
            environment.Longitude = (decimal)_dto.Longitude.Value;
        }

        if (_dto.RentalUnit is not null)
        {
            environment.RentalUnit = _dto.RentalUnit!;
        }

        if (_dto.Capacity.HasValue)
        {
            environment.Capacity = _dto.Capacity.Value;
        }

        if (_dto.InstantBooking.HasValue)
        {
            environment.InstantBooking = _dto.InstantBooking.Value;
        }

        if (_dto.MinRentalTime.HasValue)
        {
            environment.MinRentalTime = _dto.MinRentalTime.Value;
        }

        if (_dto.MaxRentalTime.HasValue)
        {
            environment.MaxRentalTime = _dto.MaxRentalTime.Value;
        }

        if (!string.IsNullOrWhiteSpace(_dto.EquipmentJson))
        {
            environment.Equipment = _dto.EquipmentJson;
        }

        if (_dto.TypePublicKey is not null)
        {
            var typeId = await _typeRepository.GetIdByPublicKeyAsync(_dto.TypePublicKey);
            environment.TypeId = typeId;
        }

        // 4) Services (si llegaron, reemplaza set)
        if (_dto.ServicePublicKeys is not null)
        {
            var map = await _serviceRepository.GetIdsByPublicKeysAsync(_dto.ServicePublicKeys);
            environment.EnvironmentServices.Clear();
            foreach (var id in map.Values)
            {
                environment.EnvironmentServices.Add(new EnvironmentService { ServiceId = id });
            }
        }

        // 5) Areas (si llegaron, reemplaza set)
        if (_dto.Areas is not null)
        {
            environment.EnvironmentAreas.Clear();

            if (_dto.Areas.Count > 0)
            {
                var keys = _dto.Areas.Select(a => a.AreaPublicKey).ToList();
                var areaMap = await _areaRepository.GetIdsByPublicKeysAsync(keys);

                foreach (var a in _dto.Areas)
                {
                    environment.EnvironmentAreas.Add(
                        new EnvironmentArea
                        {
                            AreaId = areaMap[a.AreaPublicKey],
                            Quantity = a.Quantity,
                        }
                    );
                }
            }
        }

        // 6) PricingPolicies
        if (_dto.PricingPolicies is not null)
        {
            environment.PricingPolicies.Clear();
            foreach (var p in _dto.PricingPolicies)
            {
                environment.PricingPolicies.Add(
                    new PricingPolicy
                    {
                        BasePrice = p.BasePrice,
                        Currency = p.Currency,
                        PriceUnit = p.PriceUnit,
                        ExtraGuestPrice = p.ExtraGuestPrice,
                        Environment = environment,
                    }
                );
            }
        }

        // 7) DiscountPolicies
        if (_dto.DiscountPolicies is not null)
        {
            environment.DiscountPolicies.Clear();
            foreach (var d in _dto.DiscountPolicies)
            {
                environment.DiscountPolicies.Add(
                    new DiscountPolicy
                    {
                        MinHours = d.MinHours,
                        DiscountPercentage = d.DiscountPercentage,
                        Environment = environment,
                    }
                );
            }
        }

        // 8) WeeklySchedules
        if (_dto.WeeklySchedules is not null)
        {
            environment.WeeklySchedules.Clear();
            foreach (var s in _dto.WeeklySchedules)
            {
                environment.WeeklySchedules.Add(
                    new WeeklySchedule
                    {
                        DayOfWeek = s.DayOfWeek,
                        StartTime = s.StartTime,
                        EndTime = s.EndTime,
                        Environment = environment,
                    }
                );
            }
        }

        // 9) Fotos existentes
        if (_dto.KeepPhotoIds is not null)
        {
            // Modo "reemplazo declarativo": elimina todo lo que no esté en KeepPhotoIds
            var keep = new HashSet<string>(_dto.KeepPhotoIds);
            var toDelete = environment.Photos.Where(p => !keep.Contains(p.FileId!)).ToList();
            foreach (var ph in toDelete)
            {
                await _repository.RemovePhotoAsync(ph);
            }
        }

        if (_dto.DeletePhotoIds is not null && _dto.DeletePhotoIds.Count > 0)
        {
            // Modo incremental: borra las listadas
            var delSet = new HashSet<string>(_dto.DeletePhotoIds);
            var toDelete = environment.Photos.Where(p => delSet.Contains(p.FileId!)).ToList();
            foreach (var ph in toDelete)
            {
                await _repository.RemovePhotoAsync(ph);
            }
        }

        // 10) Subida de nuevas imágenes
        if (_dto.Images?.Count > 0)
        {
            var uploaded = await _imageStorageService.UploadImagesAsync(
                _dto.Images,
                "spacio",
                "environments"
            );
            foreach (var up in uploaded)
            {
                await _repository.AddImageAsync(
                    new EnvironmentPhoto
                    {
                        EnvironmentId = environment.Id,
                        Url = up.FileUrl,
                        FileName = up.FileName,
                        FileId = up.FileId,
                    }
                );
            }
        }

        // 11) Solicitud de tour 360 (si cambió)
        if (
            _dto.Request360Tour.HasValue
            && _dto.Request360Tour.Value
            && environment.Tour360Id == null
        )
        {
            await _adminServiceAdapter.RequestTourAsync(environment.PublicId, _userId);
        }

        await _repository.SaveChangesAsync();
        return environment;
    }
}
