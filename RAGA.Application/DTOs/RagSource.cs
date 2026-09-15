using System;
using System.Collections.Generic;
using System.Text;

namespace RAGA.Application.DTOs
{
    public class RagSource
    {
        public int DocumentId { get; set; }

        public string DocumentName { get; set; } = string.Empty;

        public int PageNumber { get; set; }
    }
}
