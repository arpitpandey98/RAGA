using System;
using System.Collections.Generic;
using System.Text;

namespace RAGA.Infrastructure.Interfaces
{
    public interface IChunker
    {
        List<string> ChunkText(
        string text,
        int maxTokens = 500,
        int overlapTokens = 50);
    }
}
