namespace EnvironmentsService.Src.Infraestructure.Data;

public class MongoDBSettings
{
    required public string ConnectionString { get; set; }

    required public string DatabaseName { get; set; }
}
