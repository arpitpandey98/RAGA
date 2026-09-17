using RAGA.Domain.Entities;

namespace RAGA.Application.Common.Extensions
{
    public static class ExtensionMapper
    {
        public static FileType MapFileExtensionToFileType(string? extension)
        {
            if (string.IsNullOrWhiteSpace(extension))
                return FileType.Txt;

            var ext = extension.TrimStart('.').ToLowerInvariant();
            return ext switch
            {
                "pdf" => FileType.PDF,
                "doc" or "docx" => FileType.Word,
                "xls" or "xlsx" => FileType.Excel,
                "txt" => FileType.Txt,
                _ => FileType.Txt
            };
        }
    }
}
