using System;
using System.Collections.Generic;
using System.Text;

namespace RAGA.Application.DTOs
{
    public class SearchChunk
    {
        public int ChunkIndex { get; set; }

        public string Content { get; set; } = string.Empty;

        public int PageNumber { get; set; }

        public float[] Embedding { get; set; } = [];
    }
}
