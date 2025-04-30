namespace EnvironmentsService.Src.Application.DTOs.Responses;

public class UploadResult
{
    required public string FileId { get; set; }

    required public string FileName { get; set; }

    required public string FileUrl { get; set; }
}
