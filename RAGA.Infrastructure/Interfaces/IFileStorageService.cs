using System;
using System.Collections.Generic;
using System.Text;

namespace RAGA.Infrastructure.Interfaces
{
    public interface IFileStorageService
    {

        Task<string> SaveFileAsync(Stream fileStream, string fileName, CancellationToken cancellationToken);
        Task<Stream> GetFileAsync(string fileUrl, CancellationToken cancellationToken);
        Task DeleteFileAsync(string fileUrl);
    }
}
