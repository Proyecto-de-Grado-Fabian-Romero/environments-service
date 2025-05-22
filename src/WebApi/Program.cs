using EnvironmentsService.Src.Application.Interfaces;
using EnvironmentsService.Src.Application.Mapping;
using EnvironmentsService.Src.Application.Pipelines;
using EnvironmentsService.Src.Application.Services;
using EnvironmentsService.Src.Application.Strategies.Concretes.GetEnvironments;
using EnvironmentsService.Src.Application.Strategies.Interfaces;
using EnvironmentsService.Src.Domain.Interfaces;
using EnvironmentsService.Src.Infraestructure.Adapters;
using EnvironmentsService.Src.Infraestructure.Data;
using EnvironmentsService.Src.Infraestructure.Repositories;
using Microsoft.EntityFrameworkCore;
using MongoDB.Driver;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontEnd", policy =>
    {
        policy.WithOrigins("http://localhost:3000")
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});

builder.Services.AddSwaggerGen();
builder.Services.AddControllers();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.Configure<MongoDBSettings>(
    builder.Configuration.GetSection("MongoDB"));

builder.Services.AddSingleton<IMongoClient>(
    s => new MongoClient(builder.Configuration.GetSection("MongoDB:ConnectionString").Value));

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
builder.Services.AddScoped<EnvironmentFilterPipeline>();

builder.Services.AddScoped<ITourService, TourService>();
builder.Services.AddScoped<ITourRepository, TourRepository>();
builder.Services.AddHttpClient<IImageStorageServiceAdapter, ImageStorageServiceAdapter>();
builder.Services.AddScoped<IObjectDetectionAdapter, ObjectDetectionAdapter>();

builder.Services.AddHttpClient<IObjectDetectionAdapter, ObjectDetectionAdapter>(client =>
{
    client.BaseAddress = new Uri("http://localhost:5151");
});

builder.Services.AddAutoMapper(typeof(EnvironmentProfile));

var app = builder.Build();
app.UseCors("AllowFrontEnd");

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.Run();
