using RAGA.Application.Common.Extensions;
using RAGA.Domain.Entities;
using RAGA.Infrastructure.Interfaces;
using Microsoft.Extensions.Hosting;

namespace RAGA.Infrastructure.Services
{
    public class LocalFileStorageService : IFileStorageService
    {
        private readonly string _uploadPath;

        public LocalFileStorageService(IHostEnvironment environment)
        {
            _uploadPath = Path.Combine(
            environment.ContentRootPath,
            "App_Data",
            "uploads");

            Directory.CreateDirectory(_uploadPath);
        }
        public async Task<Stream> GetFileAsync(string fileUrl, CancellationToken cancellationToken)
        {
            var fullPath = Path.Combine(
           Directory.GetCurrentDirectory(),
           fileUrl);

            if (!File.Exists(fullPath))
                throw new FileNotFoundException("File not found.", fullPath);

            var memoryStream = new MemoryStream();

            await using var fileStream = new FileStream(
                fullPath,
                FileMode.Open,
                FileAccess.Read,
                FileShare.Read);

            await fileStream.CopyToAsync(memoryStream, cancellationToken);

            memoryStream.Position = 0;

            return memoryStream;
        }
        public async Task<string> SaveFileAsync(Stream fileStream, string fileName, CancellationToken cancellationToken)
        {
            string fileExtension = Path.GetExtension(fileName);

            var allowedExtensions = Enum.GetValues<FileType>()
                             .Select(x => x.Description())
                             .ToHashSet(StringComparer.OrdinalIgnoreCase);

            if (!allowedExtensions.Contains(fileExtension))
            {
                throw new ArgumentException(
                        "Unsupported file type. Allowed file types are: .pdf, .docx, .xlsx, .txt.",
                        nameof(fileName));
            }

            var uniqueFileName = $"{Guid.NewGuid()}-{fileName}";

            var fullPath = Path.Combine(_uploadPath, uniqueFileName);

            await using var destinationStream = new FileStream(
                fullPath,
                FileMode.Create,
                FileAccess.Write,
                FileShare.None);

            await fileStream.CopyToAsync(destinationStream, cancellationToken);
    
            // Store relative path in DB
            return Path.Combine("App_Data", "uploads", uniqueFileName)
                .Replace("\\", "/");
        }
        public Task DeleteFileAsync(string fileUrl)
        {
            var fullPath = Path.Combine(
           Directory.GetCurrentDirectory(),
           fileUrl);

            if (File.Exists(fullPath))
            {
                File.Delete(fullPath);
            }

            return Task.CompletedTask;
        }
    }
}
