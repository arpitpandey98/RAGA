using Azure.Identity;
using Azure.Monitor.OpenTelemetry.Exporter;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Identity.Web;
using Microsoft.OpenApi;
using OpenTelemetry.Trace;
using RAGA.Application.Common.Extensions;
using RAGA.Application.Interfaces;
using RAGA.Infrastructure.Data;
using RAGA.Infrastructure.Interfaces;
using RAGA.Infrastructure.Services;
using StackExchange.Redis;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenTelemetry()
    .WithTracing(tracing =>
    {
        tracing
            .AddAspNetCoreInstrumentation()
            .AddHttpClientInstrumentation()
            .AddSource("RAGA")
            .AddAzureMonitorTraceExporter(options =>
                {
                    options.ConnectionString =
                    builder.Configuration[
                    "ApplicationInsights:ConnectionString"];
                });
    });

var keyVaultUri = builder.Configuration["KeyVault:VaultUri"];

if (!string.IsNullOrWhiteSpace(keyVaultUri))
{
    builder.Configuration.AddAzureKeyVault(
        new Uri(keyVaultUri),
        new DefaultAzureCredential());
}

builder.Services.AddControllers();

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddMicrosoftIdentityWebApi(builder.Configuration, "AzureAd");

builder.Services.AddAuthorization();
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    options.OnRejected = async (context, cancellationToken) =>
    {
        context.HttpContext.Response.Headers["Retry-After"] = "60";

        await context.HttpContext.Response.WriteAsync(
            "Too many requests. Please try again later.",
            cancellationToken);
    };

    options.AddPolicy("ApiPolicy", httpContext =>
    {
        var userId = httpContext.User.Identity?.IsAuthenticated == true
        ? httpContext.User.GetUserObjectId()
        : "anonymous";

        var partitionKey = string.IsNullOrEmpty(userId)
            ? "anonymous"
            : userId;

        return RateLimitPartition.GetFixedWindowLimiter(
            partitionKey,
            _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = builder.Configuration.GetValue<int>("RateLimiting:PermitLimit"),
                Window = TimeSpan.FromSeconds(builder.Configuration.GetValue<int>("RateLimiting:WindowInSeconds")),
                QueueLimit = 0,
                AutoReplenishment = true
            });
    });
});

builder.Services.AddDbContext<RAGADbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

var azureRedisEndpoint =
    builder.Configuration["Redis:Endpoint"];

if (!string.IsNullOrWhiteSpace(azureRedisEndpoint))
{
    var redisOptions =
        StackExchange.Redis.ConfigurationOptions.Parse(
            azureRedisEndpoint);

    await redisOptions.ConfigureForAzureWithTokenCredentialAsync(
        new DefaultAzureCredential());

    builder.Services.AddStackExchangeRedisCache(options =>
    {
        options.ConfigurationOptions = redisOptions;
    });
}
else
{
    builder.Services.AddStackExchangeRedisCache(options =>
    {
        options.Configuration =
            builder.Configuration["Redis:ConnectionString"];
    });
}

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
        policy.WithOrigins("http://localhost:4200",
                            "https://lemon-smoke-0d29a2d0f1.azurestaticapps.net")
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
builder.Services.AddScoped<IConversationCacheService, RedisConversationCacheService>();
builder.Services.AddScoped<IRagCacheService, RedisRagCacheService>();

var app = builder.Build();

app.UseCors("AngularDev");

if (app.Environment.IsDevelopment() ||
    builder.Configuration.GetValue<bool>("Swagger:Enabled"))
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
app.UseRouting();
app.UseAuthentication();
app.UseRateLimiter();
app.UseAuthorization();
app.MapControllers();

app.MapGet("/", () => "Hello World");

//to test if Redis is working, uncomment the following code and call the endpoint /api/test/redis
//app.MapGet("/api/test/redis", async (IDistributedCache cache) =>
//{
//    const string key = "raga:redis:test";

//await cache.SetStringAsync(
//    key,
//    "Redis is working!");

//var value = await cache.GetStringAsync(key);

//return Results.Ok(new
//{
//    value
//});
//});

app.Run();
