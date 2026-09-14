using Azure.Identity;
using OpenAI;
using OpenAI.Embeddings;
using RAGA.Infrastructure.Interfaces;
using System.ClientModel.Primitives;

namespace RAGA.Infrastructure.Services
{
    public class AzureOpenAIEmbeddingService : IEmbeddingService
    {
        private readonly EmbeddingClient _embeddingClient;

        public AzureOpenAIEmbeddingService()
        {
            var endpoint = new Uri(
                "https://raga-foundry.openai.azure.com/openai/v1/");

            BearerTokenPolicy tokenPolicy = new(
                new DefaultAzureCredential(),
                "https://ai.azure.com/.default");

#pragma warning disable OPENAI001

            _embeddingClient = new EmbeddingClient(
                model: "text-embedding-3-small",
                authenticationPolicy: tokenPolicy,
                options: new OpenAIClientOptions
                {
                    Endpoint = endpoint
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
