using System.Net.Http.Headers;
using System.Text.Json;
using EnvironmentsService.Src.Application.DTOs.Responses;
using EnvironmentsService.Src.Application.Interfaces;

namespace EnvironmentsService.Src.Infraestructure.Adapters;

public class ImageStorageServiceAdapter(HttpClient httpClient) : IImageStorageServiceAdapter
{
    private readonly HttpClient _httpClient = httpClient;
    private readonly string _uploadEndpoint = "/api/image/upload-multiple";

    public async Task<List<UploadResult>> UploadImagesAsync(List<IFormFile> files, string bucket, string folder)
    {
        using var content = new MultipartFormDataContent();

        foreach (var file in files)
        {
            var streamContent = new StreamContent(file.OpenReadStream());
            streamContent.Headers.ContentType = new MediaTypeHeaderValue(file.ContentType);
            content.Add(streamContent, "files", file.FileName);
        }

        var url = $"{_uploadEndpoint}?bucket={bucket}&folder={folder}";

        var response = await _httpClient.PostAsync(url, content);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync();
        var results = JsonSerializer.Deserialize<List<UploadResult>>(json, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
        });

        return results ?? new List<UploadResult>();
    }
}
