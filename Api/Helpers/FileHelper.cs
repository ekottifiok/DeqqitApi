namespace Api.Helpers;

public static class FileHelper
{
    public const string FileLocation = "wwwroot";

    public const string FileImageUpload = "images";

    public static readonly string[] FileImageExtensions =
        [".jpg", ".jpeg", ".png", ".gif", ".bmp", ".webp", ".heic", ".heif", ".avif", ".svg", ".tiff", ".tif", ".raw"];

    // Upload file with validation
    public static async Task<(bool Success, string Message, string? FilePath)> UploadFileAsync(IFormFile? file,
        string uploadPath, IEnumerable<string> allowedExtensions, long maxFileSize = 20 * 1024 * 1024)
    {
        string uploadDir = Path.Combine(FileLocation, uploadPath);
        // Create Directory if it doesnt exists
        Directory.CreateDirectory(FileLocation);
        Directory.CreateDirectory(uploadDir);

        if (file == null || file.Length == 0) return (false, "No file uploaded", null);

        if (file.Length > maxFileSize)
            return (false, $"File size exceeds {maxFileSize / 1024 / 1024}MB limit", null);

        string extension = Path.GetExtension(file.FileName).ToLower();
        if (!allowedExtensions.Contains(extension))
            return (false, "Invalid file type", null);

        string fileName = $"{Guid.NewGuid()}{extension}";
        string filePath = Path.Combine(uploadDir, fileName);

        await using FileStream stream = new(filePath, FileMode.Create);
        await file.CopyToAsync(stream);
        return (true, "File uploaded successfully", Path.Combine(uploadPath, fileName));
    }
}