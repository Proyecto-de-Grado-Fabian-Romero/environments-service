using EnvironmentsService.Src.Application.Interfaces;
using EnvironmentsService.Src.Application.Mapping;
using EnvironmentsService.Src.Application.Pipelines;
using EnvironmentsService.Src.Application.Ports;
using EnvironmentsService.Src.Application.Services;
using EnvironmentsService.Src.Application.Strategies.Concretes.GetEnvironments;
using EnvironmentsService.Src.Application.Strategies.Interfaces;
using EnvironmentsService.Src.Application.Validator;
using EnvironmentsService.Src.Domain.Interfaces;
using EnvironmentsService.Src.Infraestructure.Adapters;
using EnvironmentsService.Src.Infraestructure.Data;
using EnvironmentsService.Src.Infraestructure.MessageBus;
using EnvironmentsService.Src.Infraestructure.Messaging;
using EnvironmentsService.Src.Infraestructure.PaymentGateway;
using EnvironmentsService.Src.Infraestructure.Repositories;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

var builder = WebApplication.CreateBuilder(args);

builder
    .Configuration.SetBasePath(builder.Environment.ContentRootPath)
    .AddJsonFile(
        $"appsettings.{builder.Environment.EnvironmentName}.json",
        optional: true,
        reloadOnChange: true
    )
    .AddEnvironmentVariables();

var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>();

builder.Services.AddCors(options =>
{
    options.AddPolicy(
        "AllowFrontend",
        policy =>
        {
            if (allowedOrigins != null && allowedOrigins.Length > 0)
            {
                policy
                    .WithOrigins(allowedOrigins)
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .AllowCredentials();
            }
            else
            {
                policy
                    .WithOrigins("http://localhost:3000", "https://localhost:3000")
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .AllowCredentials();
            }
        }
    );
});

builder.Services.AddSwaggerGen();
builder.Services.AddControllers();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"))
);

builder.Services.Configure<MongoDBSettings>(builder.Configuration.GetSection("MongoDB"));

builder.Services.AddSingleton<IMongoClient>(s => new MongoClient(
    builder.Configuration.GetSection("MongoDB:ConnectionString").Value
));

builder.Services.AddScoped<IEnvironmentService, EnvironmentService>();
builder.Services.AddScoped<IEnvironmentRepository, EnvironmentRepository>();
builder.Services.AddScoped<IAreaService, AreaService>();
builder.Services.AddScoped<IAreaRepository, AreaRepository>();
builder.Services.AddScoped<IServiceService, ServiceService>();
builder.Services.AddScoped<IServiceRepository, ServiceRepository>();
builder.Services.AddScoped<ITypeRepository, TypeRepository>();

builder.Services.AddScoped<DbContext, AppDbContext>();
builder.Services.AddScoped<IEnvironmentFilterStrategy, LocationFilterStrategy>();
builder.Services.AddScoped<IEnvironmentFilterStrategy, EnvironmentTypeFilterStrategy>();
builder.Services.AddScoped<IEnvironmentFilterStrategy, InstantBookingFilterStrategy>();
builder.Services.AddScoped<IEnvironmentFilterStrategy, CapacityFilterStrategy>();
builder.Services.AddScoped<IEnvironmentFilterStrategy, PriceFilterStrategy>();
builder.Services.AddScoped<IEnvironmentFilterStrategy, ServiceFilterStrategy>();
builder.Services.AddScoped<IEnvironmentFilterStrategy, AreaFilterStrategy>();
builder.Services.AddScoped<IEnvironmentFilterStrategy, DateAvailabilityFilterStrategy>();
builder.Services.AddScoped<IEnvironmentPostFilterStrategy, FilterByEquipmentStrategy>();
builder.Services.AddScoped<EnvironmentFilterPipeline>();
builder.Services.AddScoped<EnvironmentPostFilterPipeline>();

builder.Services.AddScoped<ITourService, TourService>();
builder.Services.AddScoped<ITourRepository, TourRepository>();
builder.Services.AddScoped<IReservationRepository, ReservationRepository>();
builder.Services.AddScoped<IReservationService, ReservationService>();
builder.Services.AddScoped<IAvailabilityRepository, AvailabilityRepository>();
builder.Services.AddScoped<IAvailabilityService, AvailabilityService>();

builder.Services.AddScoped<IImageStorageServiceAdapter, ImageStorageServiceAdapter>();
builder.Services.AddScoped<IObjectDetectionAdapter, ObjectDetectionAdapter>();
builder.Services.AddScoped<IAdminServiceAdapter, AdminServiceAdapter>();

builder.Services.AddHttpClient<LibelulaGateway>();
builder.Services.AddScoped<IPaymentGateway, LibelulaGateway>();
builder.Services.AddScoped<IPaymentService, PaymentService>();

builder.Services.AddValidatorsFromAssemblyContaining<CreateReservationRequestValidator>();
builder.Services.AddFluentValidationAutoValidation();

var imageStorageBaseUrl = builder.Configuration["Services:ImageStorage:BaseUrl"];
var adminServiceBaseUrl = builder.Configuration["Services:Admin:BaseUrl"];

builder.Services.AddHttpClient<IImageStorageServiceAdapter, ImageStorageServiceAdapter>(client =>
{
    client.BaseAddress = new Uri(imageStorageBaseUrl ?? "http://localhost:5116");
});

builder.Services.AddHttpClient<IAdminServiceAdapter, AdminServiceAdapter>(client =>
{
    client.BaseAddress = new Uri(adminServiceBaseUrl ?? "http://localhost:5101");
});

builder.Services.Configure<RabbitMqOptions>(builder.Configuration.GetSection("RabbitMQ"));

builder.Services.AddHostedService<EnvironmentsAmqpConsumer>();

builder.Services.AddScoped<INotificationsPublisher>(sp =>
{
    var opt = sp.GetRequiredService<IOptions<RabbitMqOptions>>().Value;
    return new NotificationsAmqpPublisher(opt);
});
builder.Services.AddScoped<IMessageBus>(sp =>
{
    var opt = sp.GetRequiredService<IOptions<RabbitMqOptions>>().Value;
    return new RabbitMQMessageBus(opt);
});

builder.Services.AddAutoMapper(typeof(EnvironmentProfile));

var app = builder.Build();
app.UseCors("AllowFrontend");

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();

app.Run();
