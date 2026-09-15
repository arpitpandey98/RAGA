using Azure.Identity;
using Microsoft.Extensions.Configuration;
using OpenAI;
using OpenAI.Embeddings;
using RAGA.Infrastructure.Interfaces;
using System.ClientModel.Primitives;

namespace RAGA.Infrastructure.Services
{
    public class AzureOpenAIEmbeddingService : IEmbeddingService
    {
        private readonly EmbeddingClient _embeddingClient;

        public AzureOpenAIEmbeddingService(IConfiguration configuration)
        {
            var endpoint =
                configuration["AzureOpenAI:Endpoint"]
                ?? throw new InvalidOperationException(
                    "AzureOpenAI:Endpoint is not configured.");

            var deployment =
                configuration["AzureOpenAI:EmbeddingDeployment"]
                ?? throw new InvalidOperationException(
                    "AzureOpenAI:EmbeddingDeployment is not configured.");

            var tokenPolicy = new BearerTokenPolicy(
                new DefaultAzureCredential(),
                "https://ai.azure.com/.default");

#pragma warning disable OPENAI001

            _embeddingClient = new EmbeddingClient(
                model: deployment,
                authenticationPolicy: tokenPolicy,
                options: new OpenAIClientOptions
                {
                    Endpoint = new Uri(endpoint)
                });

#pragma warning restore OPENAI001
        }

        public async Task<float[]> GenerateEmbeddingAsync(
            string text,
            CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                throw new ArgumentException(
                    "Text cannot be empty.",
                    nameof(text));
            }

            ct.ThrowIfCancellationRequested();

            var embedding =
                await _embeddingClient.GenerateEmbeddingAsync(text);

            return embedding.Value
                .ToFloats()
                .ToArray();
        }
    }
}
