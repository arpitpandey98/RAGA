using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RAGA.Infrastructure.Data;

namespace RAGA.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ConversationsController : ControllerBase
{
    private readonly RAGADbContext _context;

    public ConversationsController(RAGADbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetConversations(
        CancellationToken ct)
    {
        var conversations =
            await _context.Conversations
                .AsNoTracking()
                .OrderByDescending(x => x.UpdatedAt)
                .Select(x => new
                {
                    x.Id,
                    x.CreatedAt,
                    x.UpdatedAt
                })
                .ToListAsync(ct);

        return Ok(conversations);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetConversation(
    int id,
    CancellationToken ct)
    {
        var conversation =
            await _context.Conversations
                .AsNoTracking()
                .Where(x => x.Id == id)
                .Select(x => new
                {
                    x.Id,
                    x.CreatedAt,
                    x.UpdatedAt,

                    Messages = x.Messages
                        .OrderBy(m => m.CreatedAt)
                        .Select(m => new
                        {
                            m.Id,
                            m.Role,
                            m.Content,
                            m.CreatedAt
                        })
                        .ToList()
                })
                .FirstOrDefaultAsync(ct);

        if (conversation == null)
        {
            return NotFound(new
            {
                message = $"Conversation {id} was not found."
            });
        }

        return Ok(conversation);
    }
}