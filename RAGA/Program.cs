using Microsoft.EntityFrameworkCore;
using RAGA.Application.Interfaces;
using RAGA.Infrastructure.Data;
using RAGA.Infrastructure.Interfaces;
using RAGA.Infrastructure.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddDbContext<RAGADbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();
builder.Services.AddSwaggerGen();

//scoped service for file storage
//builder.Services.AddScoped<IFileStorageService, LocalFileStorageService>(); // moved to azure blob storage
builder.Services.AddScoped<ITextExtractor, PdfTextExtractor>();
builder.Services.AddScoped<IChunker, DocumentChunker>();
builder.Services.AddScoped<IEmbeddingService, AzureOpenAIEmbeddingService>();
builder.Services.AddScoped<IDocumentProcessingService, DocumentProcessingService>();
builder.Services.AddScoped<IDocumentSearchService, DocumentSearchService>();
builder.Services.AddScoped<ISearchIndexService, AzureSearchIndexService>();
builder.Services.AddScoped<IFileStorageService, AzureBlobFileStorageService>();
builder.Services.AddScoped<IChatCompletionService, AzureOpenAIChatService>();
builder.Services.AddScoped<IRagService, RagService>();
builder.Services.AddScoped<IConversationService, ConversationService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.MapGet("/", () => "Hello World!");

app.Run();
