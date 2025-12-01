using FluentValidation;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using JackLimited.Application;
using JackLimited.Domain;
using JackLimited.Infrastructure;
using Microsoft.EntityFrameworkCore;
using System.Runtime.CompilerServices;
using AspNetCoreRateLimit;
using Microsoft.AspNetCore.HttpOverrides;

[assembly: InternalsVisibleTo("JackLimited.Tests")]

var builder = WebApplication.CreateBuilder(args);

// Add user secrets in development
if (builder.Environment.IsEnvironment("Development"))
{
    builder.Configuration.AddUserSecrets<Program>();
}

// Add services
builder.Services.AddValidatorsFromAssembly(typeof(SurveyRequest).Assembly);

// Configure rate limiting
builder.Services.AddMemoryCache();
builder.Services.Configure<IpRateLimitOptions>(builder.Configuration.GetSection("IpRateLimiting"));
builder.Services.AddSingleton<IIpPolicyStore, MemoryCacheIpPolicyStore>();
builder.Services.AddSingleton<IRateLimitCounterStore, MemoryCacheRateLimitCounterStore>();
builder.Services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();
builder.Services.AddSingleton<IProcessingStrategy, AsyncKeyLockProcessingStrategy>();

if (builder.Environment.IsEnvironment("Testing") || builder.Environment.IsEnvironment("Development"))
{
    builder.Services.AddDbContext<JackLimitedDbContext>(options =>
        options.UseInMemoryDatabase("TestDb"));
}
else
{
    builder.Services.AddDbContext<JackLimitedDbContext>(options =>
        options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));
}

builder.Services.AddScoped<ISurveyRepository, SurveyRepository>();

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins("http://localhost:5173", "http://localhost:5174", "https://jacklimited-portal.azurewebsites.net") // Vite dev server + production
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

// Configure forwarded headers (important for proxies like Azure App Service)
app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = Microsoft.AspNetCore.HttpOverrides.ForwardedHeaders.XForwardedFor | Microsoft.AspNetCore.HttpOverrides.ForwardedHeaders.XForwardedProto
});

// Configure the HTTP request pipeline
if (!app.Environment.IsDevelopment())
{
    // Enable HSTS in production
    app.UseHsts();
}

// Use rate limiting
app.UseIpRateLimiting();

// Add security headers
app.Use(async (context, next) =>
{
    // Content Security Policy
    context.Response.Headers["Content-Security-Policy"] = 
        "default-src 'self'; " +
        "script-src 'self' 'unsafe-inline' 'unsafe-eval'; " +
        "style-src 'self' 'unsafe-inline'; " +
        "img-src 'self' data: https:; " +
        "font-src 'self'; " +
        "connect-src 'self'; " +
        "frame-ancestors 'none';";

    // Prevent clickjacking
    context.Response.Headers["X-Frame-Options"] = "DENY";

    // Prevent MIME type sniffing
    context.Response.Headers["X-Content-Type-Options"] = "nosniff";

    // Enable XSS protection
    context.Response.Headers["X-XSS-Protection"] = "1; mode=block";

    // Referrer policy
    context.Response.Headers["Referrer-Policy"] = "strict-origin-when-cross-origin";

    // Permissions policy
    context.Response.Headers["Permissions-Policy"] = "geolocation=(), microphone=(), camera=()";

    await next();
});

// Configure static files serving
app.UseDefaultFiles();
app.UseStaticFiles();

// Use CORS
app.UseCors();

app.MapGet("/", () => "Hello World!");

// Minimal API endpoint with enhanced error handling
app.MapPost("/api/survey", async (SurveyRequest request, IValidator<SurveyRequest> validator, ISurveyRepository repository) =>
{
    try
    {
        var validationResult = await validator.ValidateAsync(request);
        if (!validationResult.IsValid)
        {
            return Results.ValidationProblem(validationResult.ToDictionary());
        }

        var sanitizedComments = string.IsNullOrEmpty(request.Comments)
            ? null
            : InputSanitizer.SanitizeText(request.Comments);
        var sanitizedEmail = InputSanitizer.SanitizeEmail(request.Email);

        var survey = new Survey
        {
            Id = Guid.NewGuid(),
            LikelihoodToRecommend = request.LikelihoodToRecommend,
            Comments = sanitizedComments,
            Email = sanitizedEmail,
            CreatedAt = DateTime.UtcNow
        };

        await repository.AddAsync(survey);

        return Results.Created($"/api/survey/{survey.Id}", new
        {
            Id = survey.Id,
            Message = "Survey submitted successfully"
        });
    }
    catch (DbUpdateException)
    {
        return Results.Problem(
            title: "Database Error",
            detail: "Failed to save survey data. Please try again.",
            statusCode: 500
        );
    }
    catch (Exception)
    {
        return Results.Problem(
            title: "Internal Server Error",
            detail: "An unexpected error occurred. Please try again later.",
            statusCode: 500
        );
    }
});

app.MapGet("/api/survey/nps", async (ISurveyRepository repository) =>
{
    try
    {
        var ratings = await repository.GetAllRatingsAsync();
        var nps = NpsCalculator.CalculateNps(ratings);
        return Results.Ok(new { Nps = nps });
    }
    catch (Exception)
    {
        return Results.Problem(
            title: "Data Retrieval Error",
            detail: "Failed to calculate NPS. Please try again later.",
            statusCode: 500
        );
    }
});

app.MapGet("/api/survey/average", async (ISurveyRepository repository) =>
{
    try
    {
        var average = await repository.GetAverageRatingAsync();
        return Results.Ok(new { Average = Math.Round(average, 2) });
    }
    catch (Exception)
    {
        return Results.Problem(
            title: "Data Retrieval Error",
            detail: "Failed to calculate average rating. Please try again later.",
            statusCode: 500
        );
    }
});

app.MapGet("/api/survey/distribution", async (ISurveyRepository repository) =>
{
    try
    {
        var ratings = await repository.GetAllRatingsAsync();
        var distribution = ratings
            .GroupBy(r => r)
            .ToDictionary(g => g.Key, g => g.Count());
        return Results.Ok(distribution);
    }
    catch (Exception)
    {
        return Results.Problem(
            title: "Data Retrieval Error",
            detail: "Failed to retrieve rating distribution. Please try again later.",
            statusCode: 500
        );
    }
});

if (app.Environment.IsEnvironment("Testing"))
{
    app.MapPost("/testing/reset", async (JackLimitedDbContext dbContext) =>
    {
        dbContext.Surveys.RemoveRange(dbContext.Surveys);
        await dbContext.SaveChangesAsync();
        return Results.Ok(new { Message = "Test data cleared" });
    });

    app.MapPost("/testing/seed", async ([FromBody] IEnumerable<SurveyRequest> seeds, IValidator<SurveyRequest> validator, JackLimitedDbContext dbContext) =>
    {
        if (seeds == null)
        {
            return Results.BadRequest(new { Error = "Seed payload is required" });
        }

        var seedList = seeds.ToList();
        foreach (var seed in seedList)
        {
            var validationResult = await validator.ValidateAsync(seed);
            if (!validationResult.IsValid)
            {
                return Results.ValidationProblem(validationResult.ToDictionary());
            }

            var sanitizedComments = string.IsNullOrEmpty(seed.Comments)
                ? null
                : InputSanitizer.SanitizeText(seed.Comments);
            var sanitizedEmail = InputSanitizer.SanitizeEmail(seed.Email);

            dbContext.Surveys.Add(new Survey
            {
                Id = Guid.NewGuid(),
                LikelihoodToRecommend = seed.LikelihoodToRecommend,
                Comments = sanitizedComments,
                Email = sanitizedEmail,
                CreatedAt = DateTime.UtcNow
            });
        }

        await dbContext.SaveChangesAsync();
        return Results.Ok(new { Count = seedList.Count });
    });
}

app.Run();

public partial class Program { }
