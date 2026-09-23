using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using RAGA.Application.Common.Extensions;
using RAGA.Domain.Entities;
using RAGA.Infrastructure.Data;
using RAGA.Infrastructure.Interfaces;

namespace RAGA.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    [EnableRateLimiting("ApiPolicy")]

    public class DocumentController : ControllerBase
    {
        private readonly IFileStorageService _fileStorageService;
        private readonly RAGADbContext _context;
        private readonly ISearchIndexService _searchIndexService;
        public DocumentController(RAGADbContext context, IFileStorageService fileStorageService, ISearchIndexService searchIndexService)
        {
            _context = context;
            _fileStorageService = fileStorageService;
            _searchIndexService = searchIndexService;
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var userObjectId = User.GetUserObjectId();

            var document = await _context.Documents
                .FirstOrDefaultAsync(
                    x => x.Id == id && x.UploadedBy == userObjectId);

            if (document == null)
                return NotFound();

            return Ok(document);
        }

        [HttpGet]
        public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
        {
            var userObjectId = User.GetUserObjectId();

            var documents = await _context.Documents.Where(x => x.UploadedBy == userObjectId).ToListAsync(cancellationToken);

            return Ok(documents);
        }

        [HttpPost]
        public async Task<IActionResult> UploadFile(IFormFile file, CancellationToken cancellationToken)
        {
            if (file == null || file.Length == 0)
                return BadRequest("Please select a file.");

            var fileName = Path.GetFileName(file.FileName);

            if (string.IsNullOrWhiteSpace(fileName))
            {
                return BadRequest("File name cannot be empty.");
            }

            if (fileName.Length > 255)
            {
                return BadRequest("File name cannot exceed 255 characters.");
            }

            string savedPath;

            try
            {
                savedPath = await _fileStorageService.SaveFileAsync(
                    file.OpenReadStream(),
                    file.FileName,
                    cancellationToken);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new
                {
                    message = ex.Message
                });
            }

            var document = new Document
            {
                FileName = fileName,
                FileType = ExtensionMapper.MapFileExtensionToFileType(Path.GetExtension(file.FileName)),
                BlobPath = savedPath,
                UploadedAt = DateTime.UtcNow,
                UploadedBy = User.GetUserObjectId(),
                Status = Status.Uploaded
            };

            _context.Documents.Add(document);
            await _context.SaveChangesAsync(cancellationToken);

            return Ok(document);
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
        {
            var userObjectId = User.GetUserObjectId();

            var document = await _context.Documents
                .FirstOrDefaultAsync(
                    x => x.Id == id && x.UploadedBy == userObjectId,
                    cancellationToken);

            if (document == null)
                return NotFound();

            // 1. Remove document chunks from Azure AI Search
            await _searchIndexService.DeleteDocumentAsync(
                id,
                cancellationToken);

            // 2. Remove the physical file from Azure Blob Storage
            await _fileStorageService.DeleteFileAsync(
                document.BlobPath);

            // 3. Remove document metadata from SQL Server
            _context.Documents.Remove(document);

            await _context.SaveChangesAsync(cancellationToken);

            return NoContent();
        }

        [HttpPost("extract")]
        public async Task<IActionResult> ExtractText(IFormFile file, ITextExtractor textExtractor, CancellationToken cancellationToken)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest("Please select a file.");
            }

            const long maxFileSize = 20 * 1024 * 1024;

            if (file.Length > maxFileSize)
            {
                return BadRequest("File size cannot exceed 20 MB.");
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
            {
                return BadRequest("Please select a file.");
            }

            const long maxFileSize = 20 * 1024 * 1024;

            if (file.Length > maxFileSize)
            {
                return BadRequest("File size cannot exceed 20 MB.");
            }

            var fileType = Path.GetExtension(file.FileName);

            await using var stream = file.OpenReadStream();

            var pages = await textExtractor.ExtractTextAsync(
                stream,
                ExtensionMapper.MapFileExtensionToFileType(fileType),
                cancellationToken);

            var chunks = chunker.ChunkPages(pages);

            return Ok(new
            {
                FileName = file.FileName,
                TotalPages = pages.Count,
                TotalCharacters = pages.Sum(x => x.Text.Length),
                TotalChunks = chunks.Count,
                Chunks = chunks.Select(x => new
                {
                    x.ChunkIndex,
                    x.PageNumber,
                    x.Content
                })
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

        //to check the current user and their claims, you can uncomment the following code:
        //[HttpGet("me")]
        //public IActionResult GetCurrentUser()
        //{
        //    return Ok(new
        //    {
        //        Name = User.Identity?.Name,
        //        Claims = User.Claims.Select(c => new
        //        {
        //            c.Type,
        //            c.Value
        //        })
        //    });
        //}

        // to delete orphan chucks of any documents 
        //[HttpDelete("document/{documentId:int}")]
        //public async Task<IActionResult> DeleteDocumentFromSearch(
        //int documentId,
        //CancellationToken ct)
        //{
        //    await _searchIndexService.DeleteDocumentAsync(
        //        documentId,
        //        ct);

        //    return NoContent();
        //}

    }

}