using RAGA.Application.Common.Extensions;
using RAGA.Domain.Entities;
using RAGA.Infrastructure.Interfaces;
using UglyToad.PdfPig;

namespace RAGA.Infrastructure.Services
{
    public class PdfTextExtractor : ITextExtractor
    {
        public Task<string> ExtractTextAsync(
        Stream fileStream,
        FileType fileType,
        CancellationToken ct)
        {
            var allowedExtensions = Enum.GetValues<FileType>()
                             .Select(x => x.Description())
                             .ToHashSet(StringComparer.OrdinalIgnoreCase);

            if (!allowedExtensions.Contains(fileType.Description()))
            {
                throw new InvalidOperationException("Unsupported file type.");
            }

            using var pdf = PdfDocument.Open(fileStream);

            var text = new System.Text.StringBuilder();

            foreach (var page in pdf.GetPages())
            {
                ct.ThrowIfCancellationRequested();

                text.AppendLine(page.Text);
            }

            return Task.FromResult(text.ToString());
        }
    }
}
