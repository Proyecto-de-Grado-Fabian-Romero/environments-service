namespace EnvironmentsService.Src.Application.DTOs.Get;

public class EnvironmentPhotoDto
{
    required public string FileId { get; set; }

    public string FileName { get; set; } = string.Empty;

    public string Url { get; set; } = string.Empty;

    public int Order { get; set; }
}
