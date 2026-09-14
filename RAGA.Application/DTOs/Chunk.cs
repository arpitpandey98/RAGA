using System;
using System.Collections.Generic;
using System.Text;

namespace RAGA.Application.DTOs
{
    public class Chunk
    {
        public int ChunkIndex { get; set; }

        public int PageNumber { get; set; }

        public string Content { get; set; } = string.Empty;
    }
}
