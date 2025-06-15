using System.Text;
using System.Text.Json;
using EnvironmentsService.Src.Application.Interfaces;

namespace EnvironmentsService.Src.Infraestructure.Adapters;

public class AdminServiceAdapter(HttpClient client) : IAdminServiceAdapter
{
    private readonly HttpClient _client = client;

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

        var request = new HttpRequestMessage(HttpMethod.Post, "/api/tour360requests")
        {
            Content = content,
        };

        request.Headers.Add("Cookie", $"publicId={ownerId}");

        var response = await _client.SendAsync(request);
        response.EnsureSuccessStatusCode();
    }

    public async Task RequestOwnerIncomeAsync(Guid ownerId, Guid reservationId, decimal amount, string currency, long generatedAt)
    {
        var payload = new
        {
            OwnerId = ownerId,
            ReservationId = reservationId,
            Amount = amount,
            Currency = currency,
            GeneratedAt = generatedAt,
        };

        var content = new StringContent(
            JsonSerializer.Serialize(payload),
            Encoding.UTF8,
            "application/json");

        var request = new HttpRequestMessage(HttpMethod.Post, "/api/owners/earnings")
        {
            Content = content,
        };

        request.Headers.Add("Cookie", $"publicId={ownerId}");

        var response = await _client.SendAsync(request);
        response.EnsureSuccessStatusCode();
    }
}
