using Microsoft.Extensions.Logging;
using RAGA.Application.DTOs;
using RAGA.Application.Interfaces;
using RAGA.Infrastructure.Interfaces;
using System.Diagnostics;
using System.Text;
using System.Text.Json;

namespace RAGA.Infrastructure.Services;

public class RagService : IRagService
{
    private readonly ISearchIndexService _searchIndexService;
    private readonly IChatCompletionService _chatCompletionService;
    private readonly IRagCacheService _ragCacheService;
    private readonly ILogger<RagService> _logger;


    private static readonly ActivitySource ActivitySource = new("RAGA");

    public RagService(
        ISearchIndexService searchIndexService,
        IChatCompletionService chatCompletionService,
        IRagCacheService ragCacheService,
        ILogger<RagService> logger)
    {
        _searchIndexService = searchIndexService;
        _chatCompletionService = chatCompletionService;
        _ragCacheService = ragCacheService;
        _logger = logger;
    }

    public async Task<RagResponse> AskAsync(
        string question,
        CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(question))
        {
            throw new ArgumentException(
                "Question cannot be empty.",
                nameof(question));
        }

        var cachedResponse = await _ragCacheService.GetAsync(question, ct);

        if (cachedResponse != null)
        {
            var cachedRagResponse =
         JsonSerializer.Deserialize<RagResponse>(
             cachedResponse);

            if (cachedRagResponse != null)
            {
                _logger.LogInformation("RAG cache hit. QuestionLength={QuestionLength}", question.Length);
                return cachedRagResponse;
            }
        }

        // 1. Retrieve relevant chunks
        using var retrievalActivity = ActivitySource.StartActivity("RAGA.Retrieval", ActivityKind.Internal);

        retrievalActivity?.SetTag("rag.question.length", question.Length);

        var searchResults = await _searchIndexService.SearchAsync(question, topK: 5, ct);

        _logger.LogInformation("RAG retrieval completed. QuestionLength={QuestionLength}, ResultCount={ResultCount}", question.Length, searchResults.Count);

        retrievalActivity?.SetTag("rag.search.results", searchResults.Count);

        // 2. If nothing relevant was found, don't ask the LLM
        if (searchResults.Count == 0)
        {
            return new RagResponse
            {
                Answer = "I don't have enough information to answer that.",
                Sources = []
            };
        }

        // 3. Build the context supplied to the LLM
        var contextBuilder = new StringBuilder();

        foreach (var result in searchResults)
        {
            contextBuilder.AppendLine(
                $"Document: {result.Title}");

            contextBuilder.AppendLine(
                $"Page: {result.PageNumber}");

            contextBuilder.AppendLine(
                $"Content: {result.Content}");

            contextBuilder.AppendLine();
        }

        var systemPrompt = $"""
            You are an enterprise knowledge assistant.

            Answer the question using ONLY the supplied context.

            If the answer isn't available in the context,
            say you don't have enough information.

            Do not use outside knowledge.
            Do not make up facts.

            Context:
            {contextBuilder}

            """;

        // 4. Ask the LLM using the grounded context
        using var generationActivity = ActivitySource.StartActivity("RAGA.Generation", ActivityKind.Internal);

        generationActivity?.SetTag("rag.question.length", question.Length);

        var answer = await _chatCompletionService.GenerateAnswerAsync(systemPrompt, question, ct);

        _logger.LogInformation("RAG generation completed. QuestionLength={QuestionLength}, AnswerLength={AnswerLength}, SourceCount={SourceCount}", 
        question.Length, answer.Length, searchResults.Count);

        generationActivity?.SetTag("rag.answer.length", answer.Length);

        await _ragCacheService.SetAsync(question, answer, ct);

        // 5. Build citations from the actual Search results
        var sources = searchResults
            .Select(result => new RagSource
            {
                DocumentId = result.DocumentId,
                DocumentName = result.Title,
                PageNumber = result.PageNumber
            })
            .DistinctBy(source => new
            {
                source.DocumentId,
                source.PageNumber
            })
            .ToList();

        var ragResponse = new RagResponse
        {
            Answer = answer,
            Sources = sources
        };

        await _ragCacheService.SetAsync(question, JsonSerializer.Serialize(ragResponse), ct);

        return new RagResponse
        {
            Answer = answer,
            Sources = sources
        };
    }
}