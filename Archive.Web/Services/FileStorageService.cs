using System.Globalization;

namespace Archive.Web.Services;

public class FileStorageService : IFileStorageService
{
    private static readonly string[] AllowedExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".webp", ".svg" };
    private const long MaxBytes = 2 * 1024 * 1024;
    private readonly IWebHostEnvironment _environment;

    public FileStorageService(IWebHostEnvironment environment)
    {
        _environment = environment;
    }

    public async Task<ServiceResult<string>> SaveImageAsync(IFormFile file, string folderName)
    {
        if (file.Length == 0)
        {
            return ServiceResult<string>.Fail("Tệp ảnh đang rỗng.");
        }

        if (file.Length > MaxBytes)
        {
            return ServiceResult<string>.Fail("Ảnh tối đa 2MB.");
        }

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!AllowedExtensions.Contains(extension))
        {
            return ServiceResult<string>.Fail("Định dạng ảnh chưa được hỗ trợ.");
        }

        var uploadsRoot = Path.Combine(_environment.WebRootPath, "uploads", folderName);
        Directory.CreateDirectory(uploadsRoot);

        var fileName = $"{DateTime.UtcNow:yyyyMMddHHmmss}_{Guid.NewGuid():N}{extension}";
        var filePath = Path.Combine(uploadsRoot, fileName);

        await using var stream = new FileStream(filePath, FileMode.Create);
        await file.CopyToAsync(stream);

        var relativePath = string.Format(
            CultureInfo.InvariantCulture,
            "/uploads/{0}/{1}",
            folderName,
            fileName);

        return ServiceResult<string>.Ok(relativePath, "Tải ảnh thành công.");
    }
}
