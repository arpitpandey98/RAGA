using Microsoft.AspNetCore.Mvc;
using RAGA.Application.Common.Extensions;
using RAGA.Application.Interfaces;
using RAGA.Infrastructure.Interfaces;
using System.ComponentModel.DataAnnotations;

namespace RAGA.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ChatController : ControllerBase
{
    //needed for testing only to see if chat model is connected and working 
    //private readonly IChatCompletionService _chatCompletionService;

    //public ChatTestController(
    //    IChatCompletionService chatCompletionService)
    //{
    //    _chatCompletionService = chatCompletionService;
    //}

    //[HttpPost]
    //public async Task<IActionResult> Test([FromBody] ChatTestRequest request, CancellationToken ct)
    //{
    //    var systemPrompt = """
    //        You are a helpful assistant.
    //        Answer the user's question clearly and briefly.
    //        """;

    //    var answer =
    //        await _chatCompletionService.GenerateAnswerAsync(
    //            systemPrompt,
    //            request.Message,
    //            ct);

    //    return Ok(new
    //    {
    //        answer
    //    });
    //}

    private readonly IRagService _ragService;
    private readonly IConversationService _conversationService;

    public ChatController(
        IRagService ragService,
        IConversationService conversationService)
    {
        _ragService = ragService;
        _conversationService = conversationService;
    }

    [HttpPost]
    public async Task<IActionResult> Chat(
        [FromBody] ChatRequest request,
        CancellationToken ct)
    {

        int conversationId;

        var userId = User.GetUserObjectId();

        // Create a new conversation when one wasn't supplied
        if (string.IsNullOrWhiteSpace(request.ConversationId))
        {
            conversationId =
                await _conversationService
                    .CreateConversationAsync(userId, ct);
        }
        else if (!int.TryParse(
            request.ConversationId,
            out conversationId))
        {
            return BadRequest(new
            {
                message = "ConversationId must be a valid integer."
            });
        }
        else
        {
            var belongsToUser =
                await _conversationService
                    .BelongsToUserAsync(
                        conversationId,
                        userId,
                        ct);

            if (!belongsToUser)
            {
                return NotFound(new
                {
                    message = "Conversation not found."
                });
            }
        }

        // Save the user's message
        try
        {
            await _conversationService.AddMessageAsync(
                conversationId,
                "user",
                request.Message,
                ct);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new
            {
                message = ex.Message
            });
        }

        // Generate the grounded RAG answer
        var response =
        await _ragService.AskAsync(
            request.Message,
            ct);

        // Save the assistant's answer
        try
        {
            await _conversationService.AddMessageAsync(
                conversationId,
                "assistant",
                response.Answer,
                ct);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new
            {
                message = ex.Message
            });
        }

        return Ok(new
        {
            conversationId,
            answer = response.Answer,
            sources = response.Sources
        });
    }
}

public class ChatRequest
{
    [MaxLength(20)]
    public string? ConversationId { get; set; }

    [Required]
    [MaxLength(4000)]
    public string Message { get; set; } = string.Empty;
}

public class ChatTestRequest
{
    public string Message { get; set; } = string.Empty;
}
