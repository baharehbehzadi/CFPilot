using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

namespace CashflowPilot.Infrastructure.Services;

public interface IFileStorageService
{
    Task<(string storedFileName, string fullPath)> SaveUploadAsync(IFormFile file, string subFolder);
    void DeleteFile(string fullPath);
    Stream OpenRead(string fullPath);
    string GetStorageRoot();
}

public class FileStorageService : IFileStorageService
{
    private readonly string _storageRoot;

    public FileStorageService(IConfiguration configuration)
    {
        _storageRoot = configuration["Storage:Root"] ?? Path.Combine(Directory.GetCurrentDirectory(), "storage");
        Directory.CreateDirectory(_storageRoot);
    }

    public async Task<(string storedFileName, string fullPath)> SaveUploadAsync(IFormFile file, string subFolder)
    {
        var folder = Path.Combine(_storageRoot, subFolder);
        Directory.CreateDirectory(folder);

        var ext = Path.GetExtension(file.FileName);
        var storedName = $"{Guid.NewGuid()}{ext}";
        var fullPath = Path.Combine(folder, storedName);

        using var stream = new FileStream(fullPath, FileMode.Create, FileAccess.Write);
        await file.CopyToAsync(stream);

        return (storedName, fullPath);
    }

    public void DeleteFile(string fullPath)
    {
        if (File.Exists(fullPath))
            File.Delete(fullPath);
    }

    public Stream OpenRead(string fullPath) => File.OpenRead(fullPath);

    public string GetStorageRoot() => _storageRoot;
}
