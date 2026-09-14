using System;
using System.Collections.Generic;
using System.Text;

namespace RAGA.Infrastructure.Interfaces
{
    public interface IEmbeddingService
    {
       Task<float[]> GenerateEmbeddingAsync(
       string text,
       CancellationToken ct);
    }
}