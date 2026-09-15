using System;
using System.Collections.Generic;
using System.Text;

namespace RAGA.Application.DTOs
{
    public class RagResponse
    {
        public string Answer { get; set; } = string.Empty;

        public List<RagSource> Sources { get; set; } = [];
    }

}
