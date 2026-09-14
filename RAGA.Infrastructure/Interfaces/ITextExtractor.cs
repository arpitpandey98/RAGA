using RAGA.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace RAGA.Infrastructure.Interfaces
{
    public interface ITextExtractor
    {
        Task<string> ExtractTextAsync(
        Stream fileStream,
        FileType fileType,
        CancellationToken ct);
    }
}
