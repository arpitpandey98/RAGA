using Microsoft.EntityFrameworkCore;
using RAGA.Application.Interfaces;
using RAGA.Domain.Entities;
using RAGA.Infrastructure.Data;
using RAGA.Infrastructure.Interfaces;

namespace RAGA.DocumentProcessor;

public class Worker : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<Worker> _logger;

    public Worker(
        IServiceScopeFactory scopeFactory,
        ILogger<Worker> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(
        CancellationToken stoppingToken)
    {
        _logger.LogInformation(
            "RAGA Document Processor started.");

        using var timer = new PeriodicTimer(
            TimeSpan.FromMinutes(2));

        // Run once immediately when the worker starts.
        await ProcessUploadedDocumentsAsync(stoppingToken);

        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            await ProcessUploadedDocumentsAsync(stoppingToken);
        }
    }

    private async Task ProcessUploadedDocumentsAsync(
        CancellationToken stoppingToken)
    {
        using var scope = _scopeFactory.CreateScope();

        var context =
            scope.ServiceProvider
                .GetRequiredService<RAGADbContext>();

        var processingService =
            scope.ServiceProvider
                .GetRequiredService<IDocumentProcessingService>();

        var documents =
            await context.Documents
                .Where(x => x.Status == Status.Uploaded)
                .OrderBy(x => x.UploadedAt)
                .ToListAsync(stoppingToken);

        if (documents.Count == 0)
        {
            _logger.LogInformation(
                "No uploaded documents found.");
            return;
        }

        _logger.LogInformation(
            "Found {Count} uploaded document(s).",
            documents.Count);

        foreach (var document in documents)
        {
            await ProcessDocumentAsync(
                document,
                context,
                processingService,
                stoppingToken);
        }
    }

    private async Task ProcessDocumentAsync(
        Document document,
        RAGADbContext context,
        IDocumentProcessingService processingService,
        CancellationToken stoppingToken)
    {
        _logger.LogInformation(
            "Starting document processing. DocumentId: {DocumentId}, FileName: {FileName}",
            document.Id,
            document.FileName);

        try
        {
            // Mark document as Processing
            document.Status = Status.Processing;

            await context.SaveChangesAsync(
                stoppingToken);

            _logger.LogInformation(
                "Document {DocumentId} status changed to Processing.",
                document.Id);

            // Run the existing document processing pipeline
            await processingService.ProcessDocumentAsync(
                document.Id,
                stoppingToken);

            // Mark document as Processed
            document.Status = Status.Processed;

            await context.SaveChangesAsync(
                stoppingToken);

            _logger.LogInformation(
                "Document {DocumentId} processed successfully.",
                document.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Failed to process document {DocumentId}.",
                document.Id);

            try
            {
                document.Status = Status.Failed;

                await context.SaveChangesAsync(
                    stoppingToken);
            }
            catch (Exception statusException)
            {
                _logger.LogError(
                    statusException,
                    "Failed to update document {DocumentId} status to Failed.",
                    document.Id);
            }
        }
    }
}