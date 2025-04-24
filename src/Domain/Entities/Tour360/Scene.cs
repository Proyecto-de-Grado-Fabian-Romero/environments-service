namespace EnvironmentsService.Src.Domain.Entities.Tour360;

public class Scene
{
    required public string Id { get; set; }

    required public string Name { get; set; }

    required public string ImageUrl { get; set; }

    public List<POI> Pois { get; set; } = [];
}