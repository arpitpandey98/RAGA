using Microsoft.Data.SqlTypes;
using System;
using System.Collections.Generic;
using System.Text;

namespace RAGA.Domain.Entities
{
    public class DocumentChunk
    {
        public int Id { get; set; }

        public int DocumentId { get; set; }

        public int ChunkIndex { get; set; }

        public string Content { get; set; } = string.Empty;

        // Initialize with a "null" vector value that the type provides;
        // replace 0 with the desired vector length.
        public SqlVector<float> Embedding { get; set; } = SqlVector<float>.CreateNull(0);

        public Document Document { get; set; } = null!;
        public int PageNumber { get; set; }
    }
}
