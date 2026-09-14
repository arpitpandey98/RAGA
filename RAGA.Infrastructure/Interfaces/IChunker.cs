using RAGA.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Text;

namespace RAGA.Infrastructure.Interfaces
{
    public interface IChunker
    {
        List<Chunk> ChunkPages(
        List<ExtractedPage> pages,
        int maxTokens = 500,
        int overlapTokens = 50);
    }
}
