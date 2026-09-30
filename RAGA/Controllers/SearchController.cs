using Microsoft.AspNetCore.Mvc;
using RAGA.Application.Common.Extensions;
using RAGA.Application.Interfaces;
using RAGA.Infrastructure.Interfaces;

namespace RAGA.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SearchController : ControllerBase
{
    private readonly IDocumentSearchService _searchService;
    private readonly ISearchIndexService _searchIndexService;
    private readonly ITenantService _tenantService;

    public SearchController(IDocumentSearchService searchService, ISearchIndexService searchIndexService, ITenantService tenantService)
    {
        _searchService = searchService;
        _searchIndexService = searchIndexService;
        _tenantService = tenantService;
    }

    [HttpGet]
    public async Task<IActionResult> Search(
        [FromQuery] string query,
        [FromQuery] int topK = 3,
        CancellationToken ct = default)
    {
        var tenantId = await _tenantService.GetCurrentTenantIdAsync(User, ct);

        var results = await _searchService.SearchAsync(
            tenantId,
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
        var tenantId = await _tenantService.GetCurrentTenantIdAsync(User, ct);

        var results = await _searchIndexService.SearchAsync(
            tenantId,
            query,
            topK,
            ct);

        return Ok(results);
    }
}