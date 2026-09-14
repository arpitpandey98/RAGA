using RAGA.Application.Common.Extensions;
using RAGA.Application.DTOs;
using RAGA.Domain.Entities;
using RAGA.Infrastructure.Interfaces;
using UglyToad.PdfPig;

namespace RAGA.Infrastructure.Services
{
    public class PdfTextExtractor : ITextExtractor
    {

        public Task<List<ExtractedPage>> ExtractTextAsync(
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


            var pages = new List<ExtractedPage>();

            using var pdf = PdfDocument.Open(fileStream);

            foreach (var page in pdf.GetPages())
            {
                ct.ThrowIfCancellationRequested();

                pages.Add(new ExtractedPage
                {
                    PageNumber = page.Number,
                    Text = page.Text
                });
            }

            return Task.FromResult(pages);
        }
    }
}
