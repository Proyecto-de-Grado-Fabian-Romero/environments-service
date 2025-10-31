namespace EnvironmentsService.Src.Infraestructure.Messaging;

public sealed class RabbitMqOptions
{
    public string ConnectionString { get; init; } = string.Empty;

    public string Exchange { get; init; } = "spacio.direct";
}
