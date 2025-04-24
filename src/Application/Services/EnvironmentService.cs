using System.Net.Http.Headers;
using System.Text.Json;
using AutoMapper;
using EnvironmentsService.Src.Application.Commands.Concretes;
using EnvironmentsService.Src.Application.DTOs.Create;
using EnvironmentsService.Src.Application.DTOs.Get;
using EnvironmentsService.Src.Application.DTOs.GetRequest;
using EnvironmentsService.Src.Application.DTOs.Responses;
using EnvironmentsService.Src.Application.Interfaces;
using EnvironmentsService.Src.Domain.Entities;
using EnvironmentsService.Src.Domain.Interfaces;

namespace EnvironmentsService.Src.Application.Services;

public class EnvironmentService(
    IEnvironmentRepository repository,
    IAreaRepository areaRepository,
    IServiceRepository serviceRepository,
    ITypeRepository typeRepository,
    IMapper mapper
) : IEnvironmentService
{
    private readonly IEnvironmentRepository _repository = repository;
    private readonly IAreaRepository _areaRepository = areaRepository;
    private readonly IServiceRepository _serviceRepository = serviceRepository;
    private readonly ITypeRepository _typeRepository = typeRepository;
    private readonly IMapper _mapper = mapper;

    public async Task<PagedResult<GetAllEnvironmentDto>> GetAvailableEnvironmentsAsync(GetAvailableEnvironmentsRequest request, int page, int limit)
    {
        var command = new GetAllEnvironmentsCommand(_repository, _mapper, request, page, limit);
        return await command.ExecuteAsync();
    }

    public async Task<EnvironmentDto?> GetSingleEnvironmentAsync(Guid publicId)
    {
        var command = new GetSingleEnvironmentCommand(_repository, _mapper, publicId);
        return await command.ExecuteAsync();
    }

    public async Task<EnvironmentDto> CreateAsync(CreateEnvironmentDto dto, Guid userId)
    {
        var environment = _mapper.Map<Domain.Entities.Environment>(dto);
        environment.Id = Guid.NewGuid();

        var typeId = await _typeRepository.GetIdByPublicKeyAsync(dto.TypePublicKey);
        environment.TypeId = typeId;

        var serviceMap = await _serviceRepository.GetIdsByPublicKeysAsync(dto.ServicePublicKeys);
        environment.EnvironmentServices = [.. serviceMap.Values.Select(id => new Domain.Entities.EnvironmentService
        {
            ServiceId = id,
        })];

        var areaKeys = dto.Areas.Select(a => a.AreaPublicKey).ToList();
        var areaMap = await _areaRepository.GetIdsByPublicKeysAsync(areaKeys);
        environment.EnvironmentAreas = [.. dto.Areas.Select(a => new EnvironmentArea
        {
            AreaId = areaMap[a.AreaPublicKey],
            Quantity = a.Quantity,
        })];

        // Deserialize equipment if present
        if (!string.IsNullOrWhiteSpace(dto.EquipmentJson))
        {
            // environment.Equipment = JsonSerializer.Deserialize<any>(dto.EquipmentJson);
        }

        // Handle 360 Tour
        if (dto.Request360Tour)
        {
            // environment.Tour360Requests.Add(new Tour360Request
            // {
            //     RequestDate = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeMilliseconds(),
            //     Status = "Pending",
            // });
        }

        await _repository.AddAsync(environment);
        await _repository.SaveChangesAsync();

        if (dto.Images?.Count > 0)
        {
            var uploadedFiles = await UploadImagesToExternalApi(dto.Images);

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

        return _mapper.Map<EnvironmentDto>(environment);
    }

    private static async Task<List<UploadResult>> UploadImagesToExternalApi(List<IFormFile> files)
    {
        using var client = new HttpClient();
        using var content = new MultipartFormDataContent();

        foreach (var file in files)
        {
            var streamContent = new StreamContent(file.OpenReadStream());
            streamContent.Headers.ContentType = new MediaTypeHeaderValue(file.ContentType);
            content.Add(streamContent, "files", file.FileName);
        }

        var endpointUrl = "http://localhost:5116/api/image/upload-multiple?bucket=spacio&folder=environments";
        var response = await client.PostAsync(endpointUrl, content);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync();
        var results = JsonSerializer.Deserialize<List<UploadResult>>(json, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
        });

        return results ?? [];
    }

    private class UploadResult
    {
        required public string FileId { get; set; }

        required public string FileName { get; set; }

        required public string FileUrl { get; set; }
    }
}
