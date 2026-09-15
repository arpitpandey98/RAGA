using Microsoft.EntityFrameworkCore;
using RAGA.Application.Interfaces;
using RAGA.DocumentProcessor;
using RAGA.Infrastructure.Data;
using RAGA.Infrastructure.Interfaces;
using RAGA.Infrastructure.Services;

var builder = Host.CreateApplicationBuilder(args);

// Database
builder.Services.AddDbContext<RAGADbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")));

// Document processing services
builder.Services.AddScoped<ITextExtractor, PdfTextExtractor>();
builder.Services.AddScoped<IChunker, DocumentChunker>();
builder.Services.AddScoped<IEmbeddingService, AzureOpenAIEmbeddingService>();
builder.Services.AddScoped<IDocumentProcessingService, DocumentProcessingService>();

// Azure AI Search
builder.Services.AddScoped<ISearchIndexService, AzureSearchIndexService>();

// Azure Blob Storage
builder.Services.AddScoped<IFileStorageService, AzureBlobFileStorageService>();

// Background worker
builder.Services.AddHostedService<Worker>();

var host = builder.Build();

host.Run();