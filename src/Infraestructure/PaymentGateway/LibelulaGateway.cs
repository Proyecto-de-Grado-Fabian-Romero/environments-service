using System.Text;
using System.Text.Json;
using EnvironmentsService.Src.Application.DTOs.GetRequest;
using EnvironmentsService.Src.Application.DTOs.Responses;
using EnvironmentsService.Src.Domain.Interfaces;

namespace EnvironmentsService.Src.Infraestructure.PaymentGateway;

public class LibelulaGateway(HttpClient httpClient, IConfiguration config) : IPaymentGateway
{
    private readonly IConfiguration _config = config;
    private readonly HttpClient _httpClient = httpClient;

    public string GatewayName => "Libelula";

    public async Task<PaymentUrlDto> GeneratePaymentUrlAsync(PaymentRequestDto req)
    {
        var payload = new
        {
            appkey = _config["Libelula:AppKey"],
            email_cliente = req.ClientEmail,
            identificador = req.ReservationId.ToString(),
            descripcion = $"Pago de reserva {req.ReservationId}",

            // nombre_cliente = req.ClientFirstName,
            // apellido_cliente = req.ClientLastName,
            nit = req.ClientNIT ?? "0",
            razon_social = req.ClientFullName,
            ci = req.ClientCI ?? "0",
            fecha_vencimiento = DateTime.UtcNow.AddDays(3).ToString("yyyy-MM-dd"),
            callback_url = $"https://tudominio.com/api/pagos/libelula/exito",
            url_retorno = $"https://tudominio.com/reservas/{req.ReservationId}/confirmado",
            emite_factura = true,
            tipo_factura = "Servicios",
            moneda = "BOB",
            lineas_detalle_deuda = new[]
            {
                new { concepto = "Reserva de ambiente", cantidad = 1, costo_unitario = req.TotalPrice },
            },
        };

        var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
        var response = await _httpClient.PostAsync("https://api.libelula.bo/rest/deuda/registrar", content);
        var body = await response.Content.ReadAsStringAsync();

        using var doc = JsonDocument.Parse(body);
        if (doc.RootElement.GetProperty("error").GetBoolean())
        {
            throw new Exception(doc.RootElement.GetProperty("mensaje").GetString());
        }

        return new PaymentUrlDto { Url = doc.RootElement.GetProperty("url_pasarela_pagos").GetString() ?? string.Empty };
    }
}
