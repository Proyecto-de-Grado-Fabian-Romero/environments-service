using EnvironmentsService.Src.Application.DTOs.Responses;

namespace EnvironmentsService.Src.Application.Interfaces;

public interface IImageStorageServiceAdapter
{
    Task<List<UploadResult>> UploadImagesAsync(List<IFormFile> files, string bucket, string folder);
}
