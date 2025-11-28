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

    public async Task<PaymentUrlDto> GeneratePaymentUrlAsync(
        PaymentRequestDto req,
        string fechaVencimiento
    )
    {
        var payload = new
        {
            appkey = "11bb10ce-68ba-4af1-8eb7-4e6624fed729",
            email_cliente = req.ClientEmail,
            identificador = req.ReservationId.ToString(),
            descripcion = $"Pago de reserva {req.ReservationId}",
            nit = req.ClientNIT ?? "0",
            razon_social = req.ClientFullName,
            ci = req.ClientCI ?? "0",
            callback_url = $"",
            fecha_vencimiento = fechaVencimiento,
            url_retorno = $"https://spacio-app.netlify.app/reservas/{req.ReservationId}/confirmado",
            emite_factura = true,
            tipo_factura = "Servicios",
            moneda = "BOB",
            lineas_detalle_deuda = new[]
            {
                new
                {
                    concepto = "Reserva de ambiente",
                    cantidad = 1,
                    costo_unitario = req.TotalPrice,
                },
            },
        };

        var content = new StringContent(
            JsonSerializer.Serialize(payload),
            Encoding.UTF8,
            "application/json"
        );
        var response = await _httpClient.PostAsync(
            "https://api.libelula.bo/rest/deuda/registrar",
            content
        );
        var body = await response.Content.ReadAsStringAsync();

        using var doc = JsonDocument.Parse(body);
        var errorElement = doc.RootElement.GetProperty("error");
        bool hasError = errorElement.ValueKind switch
        {
            JsonValueKind.True => true,
            JsonValueKind.False => false,
            JsonValueKind.Number => errorElement.GetInt32() != 0,
            JsonValueKind.String => bool.TryParse(errorElement.GetString(), out var b)
                ? b
                : errorElement.GetString() != "0",
            _ => false,
        };

        if (hasError)
        {
            var message = doc.RootElement.TryGetProperty("mensaje", out var msgEl)
                ? msgEl.GetString()
                : "Unknown error";
            throw new Exception(message);
        }

        return new PaymentUrlDto
        {
            Url = doc.RootElement.GetProperty("url_pasarela_pagos").GetString() ?? string.Empty,
        };
    }

    public async Task<PaymentStatusResponse> CheckPaymentStatusAsync(string identificador)
    {
        var payload = new { appkey = "11bb10ce-68ba-4af1-8eb7-4e6624fed729", identificador };

        var content = new StringContent(
            JsonSerializer.Serialize(payload),
            Encoding.UTF8,
            "application/json"
        );
        var response = await _httpClient.PostAsync(
            "https://api.libelula.bo/rest/deuda/consultar_deudas/por_identificador",
            content
        );
        var body = await response.Content.ReadAsStringAsync();

        using var doc = JsonDocument.Parse(body);

        JsonElement datos;

        var datosElement = doc.RootElement.GetProperty("datos");

        if (datosElement.ValueKind == JsonValueKind.Array && datosElement.GetArrayLength() > 0)
        {
            datos = datosElement[0];
        }
        else if (datosElement.ValueKind == JsonValueKind.Object)
        {
            datos = datosElement;
        }
        else
        {
            throw new Exception(
                "La respuesta de Libelula no contiene datos válidos de la transacción."
            );
        }

        var pagado = datos.GetProperty("pagado").GetBoolean();

        string? invoiceUrl = null;
        if (
            pagado
            && datos.TryGetProperty("facturas", out var facturas)
            && facturas.GetArrayLength() > 0
        )
        {
            invoiceUrl = facturas[0].GetProperty("url").GetString();
        }

        long? paidAt = null;
        if (pagado && datos.TryGetProperty("fecha_pago", out var fecha))
        {
            var parsedDate = DateTime.Parse(fecha.GetString()!).ToUniversalTime();
            paidAt = new DateTimeOffset(parsedDate).ToUnixTimeSeconds();
        }

        return new PaymentStatusResponse
        {
            Status = pagado ? "paid" : "pending",
            InvoiceUrl = invoiceUrl ?? string.Empty,
            PaidAt = paidAt ?? 0,
        };
    }
}
