using System;
using System.Collections.Generic;
using System.Text;

namespace RAGA.Infrastructure.Interfaces
{
    public interface IDocumentProcessingService
    {
       Task ProcessDocumentAsync(
       int documentId,
       CancellationToken ct);
    }
}
