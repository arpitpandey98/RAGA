using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RAGA.Application.Common.Extensions;
using RAGA.Domain.Entities;
using RAGA.Infrastructure.Data;
using RAGA.Infrastructure.Interfaces;
using System.IO;

namespace RAGA.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DocumentController : ControllerBase
    {
        private readonly IFileStorageService _fileStorageService;
        private readonly RAGADbContext _context;
        public DocumentController(RAGADbContext context, IFileStorageService fileStorageService)
        {
            _context = context;
            _fileStorageService = fileStorageService;
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var document = await _context.Documents.FindAsync(id);

            if (document == null)
                return NotFound();

            return Ok(document);
        }

        [HttpGet]
        public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
        {
            var documents = await _context.Documents.ToListAsync(cancellationToken);

            return Ok(documents);
        }

        [HttpPost]
        public async Task<IActionResult> UploadFile(
        IFormFile file,
        CancellationToken cancellationToken)
        {
            if (file == null || file.Length == 0)
                return BadRequest("Please select a file.");

            const long maxFileSize = 20 * 1024 * 1024;

            if (file.Length > maxFileSize)
                return BadRequest("File size cannot exceed 20 MB.");

            var savedPath = await _fileStorageService.SaveFileAsync(file.OpenReadStream(), file.FileName, cancellationToken);

            var document = new Document
            {
                FileName = file.FileName,
                FileType = ExtensionMapper.MapFileExtensionToFileType(Path.GetExtension(file.FileName)),
                BlobPath = savedPath,
                UploadedAt = DateTime.UtcNow,
                UploadedBy = "Admin",
                Status = Status.Uploaded
            };

            _context.Documents.Add(document);
            await _context.SaveChangesAsync(cancellationToken);

            return Ok("document uploaded successfully");
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
        {
            var document = await _context.Documents.FindAsync(id);

            if (document == null)
                return NotFound();

            await _fileStorageService.DeleteFileAsync(document.BlobPath);

            _context.Documents.Remove(document);

            await _context.SaveChangesAsync(cancellationToken);

            return NoContent();
        }

        [HttpPost("extract")]
        public async Task<IActionResult> ExtractText(IFormFile file, ITextExtractor textExtractor,  CancellationToken cancellationToken)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest("Please select a file.");
            }

            var fileType = Path.GetExtension(file.FileName);

            await using var stream = file.OpenReadStream();

            var text = await textExtractor.ExtractTextAsync(
                stream,
                ExtensionMapper.MapFileExtensionToFileType(fileType),
                cancellationToken);

            return Ok(new
            {
                FileName = file.FileName,
                Text = text
            });
        }

        [HttpPost("chunk")]
        public async Task<IActionResult> ChunkFile(IFormFile file, ITextExtractor textExtractor, IChunker chunker, CancellationToken cancellationToken)
        {
            if (file == null || file.Length == 0)
                return BadRequest("Please select a file.");

            var fileType = Path.GetExtension(file.FileName);

            await using var stream = file.OpenReadStream();

            var text = await textExtractor.ExtractTextAsync(
                stream,
                ExtensionMapper.MapFileExtensionToFileType(fileType),
                cancellationToken);

            var chunks = chunker.ChunkText(text);

            return Ok(new
            {
                FileName = file.FileName,
                TotalCharacters = text.Length,
                TotalChunks = chunks.Count,
                Chunks = chunks
            });
        }

        [HttpPost("{id}/process")]
        public async Task<IActionResult> ProcessDocument(int id, [FromServices] IDocumentProcessingService processingService, CancellationToken ct)
        {
            await processingService.ProcessDocumentAsync(id, ct);

            return Ok(new
            {
                message = "Document processed successfully.",
                documentId = id
            });
        }
    }

}
