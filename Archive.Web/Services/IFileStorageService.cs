namespace Archive.Web.Services;

public interface IFileStorageService
{
    Task<ServiceResult<string>> SaveImageAsync(IFormFile file, string folderName);
}
