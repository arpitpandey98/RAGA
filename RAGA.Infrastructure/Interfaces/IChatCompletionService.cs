using System;
using System.Collections.Generic;
using System.Text;

namespace RAGA.Infrastructure.Interfaces
{
    public interface IChatCompletionService
    {
        Task<string> GenerateAnswerAsync(
            string systemPrompt,
            string userMessage,
            CancellationToken ct);
    }
}
