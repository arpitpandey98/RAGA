using System;
using System.Collections.Generic;
using System.Text;

namespace RAGA.Application.DTOs
{
    public class ExtractedPage
    {
        public int PageNumber { get; set; }

        public string Text { get; set; } = string.Empty;
    }
}
