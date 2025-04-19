using System.Text.Json;
using AutoMapper;
using EnvironmentsService.Src.Application.DTOs.Get;
using EnvironmentsService.Src.Application.DTOs.GetRequest;
using EnvironmentsService.Src.Application.Interfaces;
using EnvironmentsService.Src.Domain.Entities;
using EnvironmentsService.src.Domain.Interfaces;

namespace EnvironmentsService.Src.Application.Services;

public class EnvironmentService(IEnvironmentRepository repository, IMapper mapper) : IEnvironmentService
{
    private readonly IEnvironmentRepository _repository = repository;
    private readonly IMapper _mapper = mapper;

    private readonly HttpClient _httpClient = new ();

    public async Task<PagedResult<GetAllEnvironmentDto>> GetAvailableEnvironmentsAsync(GetAvailableEnvironmentsRequest request, int page, int limit)
    {
        var (environments, totalItems) = await _repository.FilterEnvironmentsAsync(request, page, limit);

        int totalPages = (int)Math.Ceiling(totalItems / (double)limit);

        var environmentDtos = _mapper.Map<List<GetAllEnvironmentDto>>(environments);

        return new PagedResult<GetAllEnvironmentDto>
        {
            Items = environmentDtos,
            CurrentPage = page,
            TotalPages = totalPages,
            Limit = limit,
            TotalItems = totalItems,
        };
    }

    public async Task<EnvironmentDto?> GetSingleEnvironment(Guid publicId)
    {
        var environment = await _repository.GetSingleEnvironment(publicId);
        var environmentDto = _mapper.Map<EnvironmentDto>(environment);

        if (environment != null)
        {
            await Parallel.ForEachAsync(
            Enumerable.Range(0, 5),
            new ParallelOptions { MaxDegreeOfParallelism = 10 },
            async (i, _) =>
            {
                var photoUrls = new List<string>();

                foreach (var photo in environment.Photos.OrderBy(p => p.Order).Take(4))
                {
                    var url = await GeneratePhotoUrlAsync(photo);
                    photoUrls.Add(url);
                }

                environmentDto.PhotoUrls = photoUrls;
            });
        }

        return environmentDto;
    }

    private async Task<string> GeneratePhotoUrlAsync(EnvironmentPhoto photo)
    {
        var requestUrl = $"http://localhost:5116/api/image/url?fileId={photo.FileId}&fileName={photo.FileName}";

        var response = await _httpClient.GetAsync(requestUrl);

        if (!response.IsSuccessStatusCode)
        {
            return string.Empty;
        }

        var content = await response.Content.ReadAsStringAsync();

        try
        {
            var json = JsonDocument.Parse(content);
            var resultUrl = json.RootElement
                .GetProperty("url")
                .GetProperty("result")
                .GetString();

            return resultUrl ?? string.Empty;
        }
        catch (Exception)
        {
            return string.Empty;
        }
    }
}
