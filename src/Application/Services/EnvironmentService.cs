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

    public async Task<IEnumerable<GetAllEnvironmentDto>> GetAvailableEnvironmentsAsync(GetAvailableEnvironmentsRequest request)
    {
        var environments = await _repository.FilterEnvironmentsAsync(request);

        var environmentDtos = _mapper.Map<List<GetAllEnvironmentDto>>(environments);

        await Parallel.ForEachAsync(
            Enumerable.Range(0, environments.Count),
            new ParallelOptions { MaxDegreeOfParallelism = 10 },
            async (i, _) =>
            {
                var environment = environments[i];
                var photoUrls = new List<string>();

                foreach (var photo in environment.Photos.OrderBy(p => p.Order).Take(4))
                {
                    var url = await GeneratePhotoUrlAsync(photo);
                    photoUrls.Add(url);
                }

                environmentDtos[i].PhotoUrls = photoUrls;
            });

        return environmentDtos;
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
