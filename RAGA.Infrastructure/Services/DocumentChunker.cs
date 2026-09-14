using RAGA.Infrastructure.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace RAGA.Infrastructure.Services
{
    public class DocumentChunker : IChunker
    {
        public List<string> ChunkText(
        string text,
        int maxTokens = 500,
        int overlapTokens = 50)
        {
            if (string.IsNullOrWhiteSpace(text))
                return [];

            if (maxTokens <= 0)
                throw new ArgumentOutOfRangeException(nameof(maxTokens));

            if (overlapTokens < 0 || overlapTokens >= maxTokens)
                throw new ArgumentOutOfRangeException(nameof(overlapTokens));

            var words = text
                .Split(
                    [' ', '\r', '\n', '\t'],
                    StringSplitOptions.RemoveEmptyEntries);

            var chunks = new List<string>();

            var step = maxTokens - overlapTokens;

            for (int start = 0; start < words.Length; start += step)
            {
                var chunkWords = words
                    .Skip(start)
                    .Take(maxTokens);

                var chunk = string.Join(' ', chunkWords);

                if (!string.IsNullOrWhiteSpace(chunk))
                {
                    chunks.Add(chunk);
                }

                if (start + maxTokens >= words.Length)
                    break;
            }

            return chunks;
        }
    }
}
