using RAGA.Application.DTOs;
using RAGA.Infrastructure.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace RAGA.Infrastructure.Services
{
    public class DocumentChunker : IChunker
    {
        public List<Chunk> ChunkPages(
        List<ExtractedPage> pages,
        int maxTokens = 500,
        int overlapTokens = 50)
        {
            if (maxTokens <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(maxTokens));
            }

            if (overlapTokens < 0 || overlapTokens >= maxTokens)
            {
                throw new ArgumentOutOfRangeException(nameof(overlapTokens));
            }

            var chunks = new List<Chunk>();
            var chunkIndex = 0;

            foreach (var page in pages)
            {
                var words = page.Text
                    .Split(
                        (char[]?)null,
                        StringSplitOptions.RemoveEmptyEntries);

                if (words.Length == 0)
                {
                    continue;
                }

                var start = 0;

                while (start < words.Length)
                {
                    var count = Math.Min(
                        maxTokens,
                        words.Length - start);

                    var content = string.Join(
                        " ",
                        words,
                        start,
                        count);

                    chunks.Add(new Chunk
                    {
                        ChunkIndex = chunkIndex++,
                        PageNumber = page.PageNumber,
                        Content = content
                    });

                    if (start + count >= words.Length)
                    {
                        break;
                    }

                    start += maxTokens - overlapTokens;
                }
            }

            return chunks;
        }
    }
}
