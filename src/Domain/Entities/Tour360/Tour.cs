namespace EnvironmentsService.Src.Domain.Entities.Tour360;

public class Tour
{
    required public string Id { get; set; } = Guid.NewGuid().ToString();

    public List<Scene> Scenes { get; set; } = [];
}
