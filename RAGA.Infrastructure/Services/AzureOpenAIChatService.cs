using Azure.Identity;
using OpenAI;
using OpenAI.Responses;
using RAGA.Infrastructure.Interfaces;
using System.ClientModel.Primitives;

namespace RAGA.Infrastructure.Services;

#pragma warning disable OPENAI001

public class AzureOpenAIChatService : IChatCompletionService
{
    private readonly ResponsesClient _responsesClient;

    private const string DeploymentName = "gpt-5-mini";

    public AzureOpenAIChatService()
    {
        var endpoint = new Uri(
            "https://raga-foundry.services.ai.azure.com/openai/v1/");

        var tokenPolicy = new BearerTokenPolicy(
            new DefaultAzureCredential(),
            "https://ai.azure.com/.default");

        _responsesClient = new ResponsesClient(
            tokenPolicy,
            new ResponsesClientOptions
            {
                Endpoint = endpoint
            });
    }

    public async Task<string> GenerateAnswerAsync(
        string systemPrompt,
        string userMessage,
        CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(systemPrompt))
        {
            throw new ArgumentException(
                "System prompt cannot be empty.",
                nameof(systemPrompt));
        }

        if (string.IsNullOrWhiteSpace(userMessage))
        {
            throw new ArgumentException(
                "User message cannot be empty.",
                nameof(userMessage));
        }

        ct.ThrowIfCancellationRequested();

        var input = $"""
            System instructions:
            {systemPrompt}

            User question:
            {userMessage}
            """;

        var response =
            await _responsesClient.CreateResponseAsync(
                DeploymentName,
                input,
                cancellationToken: ct);

        return response.Value.GetOutputText();
    }
}

#pragma warning restore OPENAI001