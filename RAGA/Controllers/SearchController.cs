using Microsoft.AspNetCore.Mvc;
using RAGA.Infrastructure.Interfaces;

namespace RAGA.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SearchController : ControllerBase
{
    private readonly IDocumentSearchService _searchService;
    private readonly ISearchIndexService _searchIndexService;

    public SearchController(IDocumentSearchService searchService, ISearchIndexService searchIndexService)
    {
        _searchService = searchService;
        _searchIndexService = searchIndexService;
    }

    [HttpGet]
    public async Task<IActionResult> Search(
        [FromQuery] string query,
        [FromQuery] int topK = 3,
        CancellationToken ct = default)
    {
        var results = await _searchService.SearchAsync(
            query,
            topK,
            ct);

        return Ok(results);
    }
    [HttpPost("index")]
    public async Task<IActionResult> CreateIndex(
        CancellationToken ct)
    {
        await _searchIndexService.EnsureIndexAsync(ct);

        return Ok(new
        {
            message = "Azure AI Search index is ready."
        });
    }

    [HttpGet("query")]
    public async Task<IActionResult> SearchService(
    [FromQuery] string query,
    [FromQuery] int topK = 5,
    CancellationToken ct = default)
    {
        var results = await _searchIndexService.SearchAsync(
            query,
            topK,
            ct);

        return Ok(results);
    }
}