using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Web;
using Microsoft.OpenApi;
using RAGA.Application.Interfaces;
using RAGA.Infrastructure.Data;
using RAGA.Infrastructure.Interfaces;
using RAGA.Infrastructure.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddMicrosoftIdentityWebApi(builder.Configuration, "AzureAd");

builder.Services.AddAuthorization();

builder.Services.AddDbContext<RAGADbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.OAuth2,
        Flows = new OpenApiOAuthFlows
        {
            AuthorizationCode = new OpenApiOAuthFlow
            {
                AuthorizationUrl = new Uri(
                    $"{builder.Configuration["AzureAd:Instance"]}{builder.Configuration["AzureAd:TenantId"]}/oauth2/v2.0/authorize"),

                TokenUrl = new Uri(
                    $"{builder.Configuration["AzureAd:Instance"]}{builder.Configuration["AzureAd:TenantId"]}/oauth2/v2.0/token"),

                Scopes = new Dictionary<string, string>
                {
                    {
                        $"api://{builder.Configuration["AzureAd:ClientId"]}/access_as_user",
                        "Access RAGA API"
                    }
                }
            }
        }
    });

    options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecuritySchemeReference("oauth2", document),
            new List<string>
            {
                $"api://{builder.Configuration["AzureAd:ClientId"]}/access_as_user"
            }
        }
    });
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("AngularDev", policy =>
    {
        policy
            .WithOrigins("http://localhost:4200")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

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

app.UseCors("AngularDev");

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.OAuthClientId(
            builder.Configuration["AzureAd:SwaggerClientId"]!);

        options.OAuthScopes(
            $"api://{builder.Configuration["AzureAd:ClientId"]}/access_as_user");

        options.OAuthScopeSeparator(" ");

        options.OAuthUsePkce();
    });
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.MapGet("/", () => "Hello World!");

app.Run();
