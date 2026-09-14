using Microsoft.AspNetCore.Mvc;
using RAGA.Infrastructure.Interfaces;

namespace RAGA.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EmbeddingsController : ControllerBase
    {
        private readonly IEmbeddingService _embeddingService;

        public EmbeddingsController(IEmbeddingService embeddingService)
        {
            _embeddingService = embeddingService;
        }

        [HttpPost("EmbeddingTest")]
        public async Task<IActionResult> TestEmbedding(
            [FromBody] string text,
            CancellationToken ct)
        {
            var embedding =
                await _embeddingService.GenerateEmbeddingAsync(text, ct);

            return Ok(new
            {
                text,
                dimensions = embedding.Length,
                firstValues = embedding.Take(5).ToArray()
            });
        }
    }
}
