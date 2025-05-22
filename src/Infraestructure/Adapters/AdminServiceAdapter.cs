using System.Text;
using System.Text.Json;
using EnvironmentsService.Src.Application.Interfaces;

namespace EnvironmentsService.Src.Infraestructure.Adapters;

public class AdminServiceAdapter(HttpClient client) : IAdminServiceAdapter
{
    private readonly HttpClient _client = client;
    private readonly string _requestUrl = "/api/tour360requests";

    public async Task RequestTourAsync(Guid environmentId, Guid ownerId)
    {
        var payload = new
        {
            environmentId,
            ownerId,
        };

        var content = new StringContent(
            JsonSerializer.Serialize(payload),
            Encoding.UTF8,
            "application/json");

        var request = new HttpRequestMessage(HttpMethod.Post, _requestUrl)
        {
            Content = content,
        };

        request.Headers.Add("Cookie", $"publicId={ownerId}");

        var response = await _client.SendAsync(request);
        response.EnsureSuccessStatusCode();
    }
}
