using RAGA.Application.DTOs;
using RAGA.Domain.Entities;

namespace RAGA.Infrastructure.Interfaces
{
        public interface ITextExtractor
        {
            Task<List<ExtractedPage>> ExtractTextAsync(
                Stream fileStream,
                FileType fileType,
                CancellationToken ct);
        }
}
