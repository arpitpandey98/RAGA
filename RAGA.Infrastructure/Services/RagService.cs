using RAGA.Application.DTOs;
using RAGA.Application.Interfaces;
using RAGA.Infrastructure.Interfaces;
using System.Text;

namespace RAGA.Infrastructure.Services;

public class RagService : IRagService
{
    private readonly ISearchIndexService _searchIndexService;
    private readonly IChatCompletionService _chatCompletionService;

    public RagService(
        ISearchIndexService searchIndexService,
        IChatCompletionService chatCompletionService)
    {
        _searchIndexService = searchIndexService;
        _chatCompletionService = chatCompletionService;
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

        // 1. Retrieve relevant chunks
        var searchResults =
            await _searchIndexService.SearchAsync(
                question,
                topK: 5,
                ct);

        // 2. If nothing relevant was found, don't ask the LLM
        if (searchResults.Count == 0)
        {
            return new RagResponse
            {
                Answer =
                    "I don't have enough information to answer that.",
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
        var answer =
            await _chatCompletionService.GenerateAnswerAsync(
                systemPrompt,
                question,
                ct);

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

        return new RagResponse
        {
            Answer = answer,
            Sources = sources
        };
    }
}