using System.ComponentModel;

namespace RAGA.Domain.Entities
{
        public enum FileType
        {
            [Description(".pdf")]
            PDF = 1,
            [Description(".docx")]
            Word,
            [Description(".xlsx")]
            Excel,
            [Description(".txt")]
            Txt
        }
}
