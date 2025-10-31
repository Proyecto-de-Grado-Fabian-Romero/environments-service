namespace EnvironmentsService.Src.Infraestructure.Messaging;

using EnvironmentsService.Src.Application.Ports;
using EnvironmentsService.Src.Domain.Events;
using RabbitMQ.Client;

public sealed class NotificationsAmqpPublisher : INotificationsPublisher, IDisposable
{
    private readonly IConnection _conn;
    private readonly IModel _ch;
    private readonly RabbitMqOptions _opt;

    public NotificationsAmqpPublisher(RabbitMqOptions opt)
    {
        _opt = opt;
        var f = new ConnectionFactory { Uri = new Uri(opt.ConnectionString) };
        _conn = f.CreateConnection();
        _ch = _conn.CreateModel();
        _ch.ExchangeDeclare(opt.Exchange, ExchangeType.Direct, durable: true, autoDelete: false);
    }

    public void PublishSendToUser(SendToUserMessage msg)
    {
        var body = System.Text.Json.JsonSerializer.SerializeToUtf8Bytes(msg);
        var props = _ch.CreateBasicProperties();
        props.DeliveryMode = 2;
        _ch.BasicPublish(_opt.Exchange, "user.send", props, body);
    }

    public void Dispose()
    {
        _ch?.Dispose();
        _conn?.Dispose();
    }
}
