using AdminService.Src.Application.DTOs.Response;
using EnvironmentsService.Src.Application.Interfaces;
using EnvironmentsService.Src.Domain.Events;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace EnvironmentsService.Src.Infraestructure.Messaging;

public class EnvironmentsAmqpConsumer(
    IOptions<RabbitMqOptions> opt,
    IServiceProvider serviceProvider
) : BackgroundService
{
    private IConnection? _conn;
    private IModel? _ch;
    private RabbitMqOptions _opt = opt.Value;
    private IServiceProvider _serviceProvider = serviceProvider;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        Console.WriteLine($"[Consumer] Connecting to RabbitMQ: {_opt.ConnectionString}");

        var f = new ConnectionFactory { Uri = new Uri(_opt.ConnectionString) };
        _conn = f.CreateConnection();
        _ch = _conn.CreateModel();

        _ch.ExchangeDeclare(_opt.Exchange, ExchangeType.Direct, durable: true, autoDelete: false);
        Console.WriteLine($"{_opt.Exchange}");

        Console.WriteLine("🌍 [EnvironmentsConsumer] Starting queue declarations...");

        _ch.QueueDeclare(
            "environments.get.details",
            durable: true,
            exclusive: false,
            autoDelete: false
        );
        _ch.QueueBind("environments.get.details", _opt.Exchange, "environments.get.details");

        _ch.QueueDeclare(
            "environments.update.objects",
            durable: true,
            exclusive: false,
            autoDelete: false
        );
        _ch.QueueBind("environments.update.objects", _opt.Exchange, "environments.update.objects");

        _ch.QueueDeclare(
            "environments.tours.upload",
            durable: true,
            exclusive: false,
            autoDelete: false
        );
        _ch.QueueBind("environments.tours.upload", _opt.Exchange, "environments.tours.upload");
        _ch.BasicQos(0, 10, global: false);

        var getDetailsConsumer = new EventingBasicConsumer(_ch);
        getDetailsConsumer.Received += async (_, ea) =>
        {
            try
            {
                var body = ea.Body.ToArray();
                var message =
                    System.Text.Json.JsonSerializer.Deserialize<GetEnvironmentDetailsMessage>(body);

                using var scope = _serviceProvider.CreateScope();
                var environmentService =
                    scope.ServiceProvider.GetRequiredService<IEnvironmentService>();
                var environment = await environmentService.GetSingleEnvironmentAsync(
                    message!.EnvironmentPublicId
                );

                var envResponse = new EnvironmentDetailsResponse
                {
                    OwnerId = environment.OwnerId,
                    Title = environment.Title,
                };

                var responseBody = System.Text.Json.JsonSerializer.SerializeToUtf8Bytes(
                    envResponse
                );
                var responseProps = _ch.CreateBasicProperties();
                responseProps.CorrelationId = ea.BasicProperties.CorrelationId;
                responseProps.DeliveryMode = 2;

                _ch.BasicPublish(
                    _opt.Exchange,
                    "environments.get.details.response",
                    responseProps,
                    responseBody
                );

                _ch!.BasicAck(ea.DeliveryTag, false);
                Console.WriteLine(
                    $"✅ [GetDetails] Response sent for CorrelationId={ea.BasicProperties.CorrelationId}"
                );
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ [GetDetails] Error: {ex.Message}");
                _ch.BasicNack(ea.DeliveryTag, false, false);
            }
        };

        _ch.BasicConsume("environments.get.details", autoAck: false, getDetailsConsumer);
        Console.WriteLine("🟢 [GetDetails] Consumer ready on queue 'environments.get.details'");

        var updateObjectsConsumer = new EventingBasicConsumer(_ch);
        updateObjectsConsumer.Received += async (model, ea) =>
        {
            Console.WriteLine($"📥 [UpdateObjects] Message received.");
            try
            {
                var body = ea.Body.ToArray();
                var message =
                    System.Text.Json.JsonSerializer.Deserialize<UpdateDetectedObjectsMessage>(body);

                using var scope = _serviceProvider.CreateScope();
                var environmentService =
                    scope.ServiceProvider.GetRequiredService<IEnvironmentService>();
                await environmentService.UpdateDetectedObjectsAsync(
                    message!.EnvironmentPublicId,
                    message.DetectedObjects
                );

                _ch.BasicAck(ea.DeliveryTag, false);
                Console.WriteLine(
                    $"✅ [UpdateObjects] Processed environmentId={message.EnvironmentPublicId}"
                );
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ [UpdateObjects] Error: {ex.Message}");
                _ch.BasicNack(ea.DeliveryTag, false, false);
            }
        };

        _ch.BasicConsume("environments.update.objects", false, updateObjectsConsumer);
        Console.WriteLine(
            "🟢 [UpdateObjects] Consumer ready on queue 'environments.update.objects'"
        );

        var uploadTourConsumer = new EventingBasicConsumer(_ch);
        uploadTourConsumer.Received += async (model, ea) =>
        {
            Console.WriteLine(
                $"📥 [UploadTour] Message received. CorrelationId={ea.BasicProperties.CorrelationId}"
            );
            try
            {
                var body = ea.Body.ToArray();
                var message = System.Text.Json.JsonSerializer.Deserialize<UploadTourMessage>(body);

                using var scope = _serviceProvider.CreateScope();
                var tourService = scope.ServiceProvider.GetRequiredService<ITourService>();
                await tourService.CreateTourAsync(
                    message!.EnvironmentPublicId,
                    message.TourUpload.Scenes
                );

                var response = new UploadTourResponse { Success = true };
                var responseBody = System.Text.Json.JsonSerializer.SerializeToUtf8Bytes(response);
                var responseProps = _ch.CreateBasicProperties();
                Console.WriteLine("🚀 [UploadTour] Preparing success response...");
                responseProps.CorrelationId = ea.BasicProperties.CorrelationId;
                responseProps.DeliveryMode = 2;

                Console.WriteLine("🚀 [UploadTour] Sending success response...");
                _ch.BasicPublish(
                    _opt.Exchange,
                    "environments.tours.upload.response",
                    responseProps,
                    responseBody
                );

                _ch.BasicAck(ea.DeliveryTag, false);
                Console.WriteLine(
                    $"✅ [UploadTour] Response sent for CorrelationId={ea.BasicProperties.CorrelationId}"
                );
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ [UploadTour] Error: {ex.Message}");
                var errorResponse = new UploadTourResponse { Success = false, Error = ex.Message };
                var responseBody = System.Text.Json.JsonSerializer.SerializeToUtf8Bytes(
                    errorResponse
                );
                var responseProps = _ch.CreateBasicProperties();
                responseProps.CorrelationId = ea.BasicProperties.CorrelationId;
                responseProps.DeliveryMode = 2;
                _ch.BasicPublish(
                    _opt.Exchange,
                    "environments.tours.upload.response",
                    responseProps,
                    responseBody
                );
                _ch.BasicAck(ea.DeliveryTag, false);
            }
        };

        _ch.BasicConsume("environments.tours.upload", false, uploadTourConsumer);
        Console.WriteLine("🟢 [UploadTour] Consumer ready on queue 'environments.tours.upload'");

        Console.WriteLine("🚀 [EnvironmentsConsumer] All consumers initialized successfully!");

        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(1000, stoppingToken);
        }
    }

    public override void Dispose()
    {
        _ch?.Close();
        _conn?.Close();
        base.Dispose();
    }
}
