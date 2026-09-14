using Azure.Storage.Blobs;
using Microsoft.Extensions.Configuration;
using RAGA.Infrastructure.Interfaces;

namespace RAGA.Infrastructure.Services;

public class AzureBlobFileStorageService : IFileStorageService
{
    private readonly BlobContainerClient _containerClient;

    public AzureBlobFileStorageService(
        IConfiguration configuration)
    {
        var connectionString =
            configuration["AzureStorage:ConnectionString"]
            ?? throw new InvalidOperationException(
                "AzureStorage:ConnectionString is not configured.");

        var containerName =
            configuration["AzureStorage:ContainerName"]
            ?? throw new InvalidOperationException(
                "AzureStorage:ContainerName is not configured.");

        var blobServiceClient =
            new BlobServiceClient(connectionString);

        _containerClient =
            blobServiceClient.GetBlobContainerClient(containerName);
    }

    public async Task<string> SaveFileAsync(
        Stream fileStream,
        string fileName,
        CancellationToken cancellationToken)
    {
        if (fileStream == null)
        {
            throw new ArgumentNullException(nameof(fileStream));
        }

        if (string.IsNullOrWhiteSpace(fileName))
        {
            throw new ArgumentException(
                "File name cannot be empty.",
                nameof(fileName));
        }

        await _containerClient.CreateIfNotExistsAsync(
            cancellationToken: cancellationToken);

        var safeFileName =
            Path.GetFileName(fileName);

        var blobName =
            $"{Guid.NewGuid():N}-{safeFileName}";

        var blobClient =
            _containerClient.GetBlobClient(blobName);

        await blobClient.UploadAsync(
            fileStream,
            overwrite: false,
            cancellationToken);

        return blobName;
    }

    public async Task<Stream> GetFileAsync(
        string fileUrl,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(fileUrl))
        {
            throw new ArgumentException(
                "File URL cannot be empty.",
                nameof(fileUrl));
        }

        var blobClient =
            _containerClient.GetBlobClient(fileUrl);

        var response =
            await blobClient.DownloadStreamingAsync(
                cancellationToken: cancellationToken);

        return response.Value.Content;
    }

    public async Task DeleteFileAsync(
        string fileUrl)
    {
        if (string.IsNullOrWhiteSpace(fileUrl))
        {
            return;
        }

        var blobClient =
            _containerClient.GetBlobClient(fileUrl);

        await blobClient.DeleteIfExistsAsync();
    }
}