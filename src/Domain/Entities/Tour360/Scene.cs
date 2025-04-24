namespace EnvironmentsService.Src.Domain.Entities.Tour360;

public class Scene
{
    required public string Id { get; set; }

    required public string Name { get; set; }

    required public string FileId { get; set; }

    required public string FileName { get; set; }

    required public string FileUrl { get; set; }

    public List<POI> Pois { get; set; } = [];
}