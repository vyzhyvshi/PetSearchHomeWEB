using Microsoft.AspNetCore.Hosting;
using PetSearchHome_WEB.Domain.Interfaces;

namespace PetSearchHome_WEB.Infrastructure;

public class LocalStorageGateway : IStorageGateway
{
    private readonly string _uploadsRoot;

    public LocalStorageGateway(IWebHostEnvironment environment)
    {
        _uploadsRoot = Path.Combine(environment.WebRootPath ?? "wwwroot", "uploads", "chat");
        Directory.CreateDirectory(_uploadsRoot);
    }

    public async Task<string> UploadAsync(string fileName, Stream content, CancellationToken cancellationToken = default)
    {
        var extension = Path.GetExtension(fileName);
        var safeFileName = $"{Guid.NewGuid():N}{extension}";
        var path = Path.Combine(_uploadsRoot, safeFileName);

        await using var fileStream = File.Create(path);
        await content.CopyToAsync(fileStream, cancellationToken);

        return $"/uploads/chat/{safeFileName}";
    }

    public Task DeleteAsync(string fileName, CancellationToken cancellationToken = default)
    {
        var trimmed = fileName.Trim();
        var localName = trimmed.StartsWith("/uploads/chat/", StringComparison.OrdinalIgnoreCase)
            ? trimmed["/uploads/chat/".Length..]
            : Path.GetFileName(trimmed);

        var path = Path.Combine(_uploadsRoot, localName);
        if (File.Exists(path))
        {
            File.Delete(path);
        }

        return Task.CompletedTask;
    }
}
