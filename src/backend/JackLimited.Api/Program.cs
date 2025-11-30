using FluentValidation;
using Microsoft.AspNetCore.Http.HttpResults;
using JackLimited.Application;
using JackLimited.Domain;
using JackLimited.Infrastructure;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddValidatorsFromAssembly(typeof(SurveyRequest).Assembly);

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
        policy.WithOrigins("http://localhost:5173", "http://localhost:5174") // Vite dev server
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

// Use CORS
app.UseCors();

// Sanitization helper
string SanitizeText(string? text)
{
    if (string.IsNullOrEmpty(text)) return text ?? "";

    // Remove HTML tags and entities
    var sanitized = Regex.Replace(text, @"<[^>]*>", "");
    sanitized = Regex.Replace(sanitized, @"&[^;]+;", "");

    // Remove control characters
    sanitized = Regex.Replace(sanitized, @"[\x00-\x1F\x7F-\x9F]", "");

    // Trim whitespace
    return sanitized.Trim();
}

app.MapGet("/", () => "Hello World!");

// Minimal API endpoint with enhanced error handling
app.MapPost("/api/survey", async (SurveyRequest request, IValidator<SurveyRequest> validator, ISurveyRepository repository) =>
{
    try
    {
        // Sanitize input
        var sanitizedRequest = new SurveyRequest(
            request.LikelihoodToRecommend,
            string.IsNullOrEmpty(request.Comments) ? null : SanitizeText(request.Comments),
            string.IsNullOrEmpty(request.Email) ? null : request.Email?.Trim().ToLowerInvariant()
        );

        var validationResult = await validator.ValidateAsync(sanitizedRequest);
        if (!validationResult.IsValid)
        {
            return Results.ValidationProblem(validationResult.ToDictionary());
        }

        var survey = new Survey
        {
            Id = Guid.NewGuid(),
            LikelihoodToRecommend = sanitizedRequest.LikelihoodToRecommend,
            Comments = sanitizedRequest.Comments,
            Email = sanitizedRequest.Email,
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

app.Run();
