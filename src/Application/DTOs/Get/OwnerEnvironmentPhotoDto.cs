namespace EnvironmentsService.Src.Application.DTOs.Get;

public class OwnerEnvironmentPhotoDto
{
    public Guid Id { get; set; }

    required public string FileId { get; set; }

    public string FileName { get; set; } = string.Empty;

    public int Order { get; set; }
}
