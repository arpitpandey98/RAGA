using System;
using System.Collections.Generic;
using System.Text;

namespace RAGA.Application.DTOs
{
    public class SearchResult
    {
        public int DocumentId { get; set; }

        public string Title { get; set; } = string.Empty;

        public string Content { get; set; } = string.Empty;

        public int PageNumber { get; set; }

        public double Score { get; set; }
        public double? RerankerScore { get; set; }
    }
}
