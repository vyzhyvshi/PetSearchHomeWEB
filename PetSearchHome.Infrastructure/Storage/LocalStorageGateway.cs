using Microsoft.AspNetCore.Hosting;
using PetSearchHome_WEB.Domain.Interfaces;

namespace PetSearchHome.Infrastructure.Gateways
{
    public class LocalStorageGateway : IStorageGateway
    {
        private readonly IWebHostEnvironment _env;

        public LocalStorageGateway(IWebHostEnvironment env)
        {
            _env = env;
        }

        public async Task<string> UploadAsync(string fileName, Stream content, CancellationToken cancellationToken = default)
        {
            var uploadsFolder = Path.Combine(_env.WebRootPath, "uploads");
            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            var uniqueFileName = $"{Guid.NewGuid()}_{fileName}";
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await content.CopyToAsync(fileStream, cancellationToken);
            }

            return $"/uploads/{uniqueFileName}";
        }

        public Task DeleteAsync(string fileName, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(fileName)) return Task.CompletedTask;

            var name = Path.GetFileName(fileName);
            var filePath = Path.Combine(_env.WebRootPath, "uploads", name);

            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }

            return Task.CompletedTask;
        }
    }
}