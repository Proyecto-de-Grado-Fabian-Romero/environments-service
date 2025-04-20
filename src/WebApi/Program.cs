using EnvironmentsService.Src.Application.Commands.Concretes;
using EnvironmentsService.Src.Application.Commands.Interfaces;
using EnvironmentsService.Src.Application.DTOs.Get;
using EnvironmentsService.Src.Application.Interfaces;
using EnvironmentsService.Src.Application.Mapping;
using EnvironmentsService.Src.Application.Pipelines;
using EnvironmentsService.Src.Application.Services;
using EnvironmentsService.Src.Application.Strategies.Concretes.GetEnvironments;
using EnvironmentsService.Src.Application.Strategies.Interfaces;
using EnvironmentsService.src.Domain.Interfaces;
using EnvironmentsService.src.Infraestructure.Data;
using EnvironmentsService.src.Infraestructure.Repositories;
using Microsoft.EntityFrameworkCore;

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
builder.Services.AddSwaggerGen();
builder.Services.AddControllers();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddScoped<IEnvironmentService, EnvironmentService>();
builder.Services.AddScoped<IEnvironmentRepository, EnvironmentRepository>();
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

builder.Services.AddAutoMapper(typeof(EnvironmentProfile));

var app = builder.Build();
app.UseCors("AllowAll");

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
