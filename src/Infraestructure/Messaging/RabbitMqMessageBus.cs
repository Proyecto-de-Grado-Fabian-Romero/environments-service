using System.Text;
using System.Text.Json;
using EnvironmentsService.Src.Application.Interfaces;
using EnvironmentsService.Src.Infraestructure.Messaging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace EnvironmentsService.Src.Infraestructure.MessageBus;

public class RabbitMQMessageBus : IMessageBus, IDisposable
{
    private readonly IConnection _connection;
    private readonly IModel _channel;
    private readonly string _replyQueueName;
    private readonly Dictionary<string, TaskCompletionSource<string>> _pendingRequests;
    private readonly RabbitMqOptions _opt;

    public RabbitMQMessageBus(RabbitMqOptions options)
    {
        _opt = options;
        _pendingRequests = new Dictionary<string, TaskCompletionSource<string>>();

        try
        {
            var factory = new ConnectionFactory { Uri = new Uri(_opt.ConnectionString) };
            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();

            _replyQueueName = "detection_responses";
            _channel.QueueDeclare(
                queue: _replyQueueName,
                durable: true,
                exclusive: false,
                autoDelete: false
            );
            StartReplyConsumer();
        }
        catch
        {
            Console.WriteLine("❌ Error al inicializar RabbitMQMessageBus");
            throw;
        }
    }

    public async Task PublishAsync<T>(T message, string queueName)
    {
        try
        {
            Console.WriteLine($"📤 Publicando mensaje en la cola: {queueName}");

            _channel.QueueDeclare(
                queue: queueName,
                durable: true,
                autoDelete: false,
                arguments: null
            );

            var json = JsonSerializer.Serialize(message);
            var body = Encoding.UTF8.GetBytes(json);

            _channel.BasicPublish(
                exchange: _opt.Exchange,
                routingKey: queueName,
                basicProperties: null,
                body: body
            );

            Console.WriteLine($"✅ Mensaje publicado correctamente en {queueName}");

            await Task.CompletedTask;
        }
        catch
        {
            Console.WriteLine($"❌ Error publicando mensaje en la cola {queueName}");
            throw;
        }
    }

    public async Task<TResponse> RequestAsync<TRequest, TResponse>(
        TRequest request,
        string requestQueue,
        string replyQueue
    )
    {
        var requestId = Guid.NewGuid().ToString();
        var tcs = new TaskCompletionSource<string>();
        _pendingRequests[requestId] = tcs;

        try
        {
            Console.WriteLine(
                $"📨 Enviando Request ID {requestId} a {requestQueue} con ReplyTo {replyQueue}"
            );

            // Aseguramos que ambas colas existen
            _channel.QueueDeclare(requestQueue, durable: true, exclusive: false, autoDelete: false);
            _channel.QueueDeclare(replyQueue, durable: true, exclusive: false, autoDelete: false);

            var props = _channel.CreateBasicProperties();
            props.ReplyTo = replyQueue;
            props.CorrelationId = requestId;

            var jsonRequest = JsonSerializer.Serialize(request);
            var body = Encoding.UTF8.GetBytes(jsonRequest);

            _channel.BasicPublish(
                exchange: _opt.Exchange,
                routingKey: requestQueue,
                basicProperties: props,
                body: body
            );

            Console.WriteLine("📤 Request enviado. Esperando respuesta...");

            var responseJson = await tcs.Task;
            Console.WriteLine($"📥 Respuesta recibida para {requestId}");

            return JsonSerializer.Deserialize<TResponse>(responseJson)!;
        }
        catch
        {
            Console.WriteLine($"❌ Error en RequestAsync para la cola {requestQueue}");
            throw;
        }
        finally
        {
            _pendingRequests.Remove(requestId);
        }
    }

    private void StartReplyConsumer()
    {
        Console.WriteLine($"👂 Iniciando consumidor de respuestas en {_replyQueueName}");

        var consumer = new EventingBasicConsumer(_channel);
        consumer.Received += (model, ea) =>
        {
            var correlationId = ea.BasicProperties.CorrelationId;

            Console.WriteLine(
                $"📩 Mensaje recibido en ReplyQueue {_replyQueueName} con CorrelationId {correlationId}"
            );

            if (_pendingRequests.TryGetValue(correlationId, out var tcs))
            {
                var body = ea.Body.ToArray();
                var response = Encoding.UTF8.GetString(body);
                tcs.SetResult(response);
                _pendingRequests.Remove(correlationId);
                Console.WriteLine("✅ Respuesta procesada para {CorrelationId}", correlationId);
            }
            else
            {
                Console.WriteLine(
                    $"⚠️ No se encontró Request pendiente para CorrelationId {correlationId}"
                );
            }
        };

        _channel.BasicConsume(queue: _replyQueueName, autoAck: true, consumer: consumer);
    }

    public void Dispose()
    {
        Console.WriteLine("♻️ Cerrando conexión RabbitMQ...");
        _channel?.Close();
        _connection?.Close();
    }
}
