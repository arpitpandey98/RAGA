using System;
using System.Collections.Generic;
using System.Text;

namespace RAGA.Application.DTOs
{
    public class DocumentSearchResult
    {
        public int DocumentId { get; set; }

        public int ChunkId { get; set; }

        public int ChunkIndex { get; set; }

        public string Content { get; set; } = string.Empty;

        public double Distance { get; set; }
    }
}
