using FluentValidation;
using Microsoft.AspNetCore.Http.HttpResults;
using JackLimited.Application;
using JackLimited.Domain;
using JackLimited.Infrastructure;
using Microsoft.EntityFrameworkCore;

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

app.MapGet("/", () => "Hello World!");

// Minimal API endpoint
app.MapPost("/api/survey", async (SurveyRequest request, IValidator<SurveyRequest> validator, ISurveyRepository repository) =>
{
    var validationResult = await validator.ValidateAsync(request);
    if (!validationResult.IsValid)
    {
        return Results.ValidationProblem(validationResult.ToDictionary());
    }

    var survey = new Survey
    {
        Id = Guid.NewGuid(),
        LikelihoodToRecommend = request.LikelihoodToRecommend,
        Comments = request.Comments,
        Email = request.Email,
        CreatedAt = DateTime.UtcNow
    };

    await repository.AddAsync(survey);

    return Results.Created($"/api/survey/{survey.Id}", new { Id = survey.Id });
});

app.MapGet("/api/survey/nps", async (ISurveyRepository repository) =>
{
    var ratings = await repository.GetAllRatingsAsync();
    var nps = NpsCalculator.CalculateNps(ratings);
    return Results.Ok(new { Nps = nps });
});

app.MapGet("/api/survey/average", async (ISurveyRepository repository) =>
{
    var average = await repository.GetAverageRatingAsync();
    return Results.Ok(new { Average = Math.Round(average, 2) });
});

app.MapGet("/api/survey/distribution", async (ISurveyRepository repository) =>
{
    var ratings = await repository.GetAllRatingsAsync();
    var distribution = ratings
        .GroupBy(r => r)
        .ToDictionary(g => g.Key, g => g.Count());
    return Results.Ok(distribution);
});

app.Run();
