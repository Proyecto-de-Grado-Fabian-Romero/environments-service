using EnvironmentsService.Src.Domain.Entities.Tour360;
using EnvironmentsService.Src.Domain.Interfaces;
using EnvironmentsService.Src.Infraestructure.Data;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace EnvironmentsService.Src.Infraestructure.Repositories;

public class TourRepository : ITourRepository
{
    private readonly IMongoCollection<Tour> _collection;

    public TourRepository(IOptions<MongoDBSettings> settings, IMongoClient client)
    {
        var database = client.GetDatabase(settings.Value.DatabaseName);
        _collection = database.GetCollection<Tour>("tours");
    }

    public async Task SaveAsync(Tour tour)
    {
        await _collection.InsertOneAsync(tour);
    }

    public async Task<Tour?> GetByIdAsync(string id)
    {
        return await _collection.Find(t => t.Id == id).FirstOrDefaultAsync();
    }
}
