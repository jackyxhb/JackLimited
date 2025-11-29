using FluentValidation;
using Microsoft.AspNetCore.Http.HttpResults;
using JackLimited.Application;
using JackLimited.Domain;
using JackLimited.Infrastructure;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddValidatorsFromAssembly(typeof(SurveyRequest).Assembly);

if (builder.Environment.IsEnvironment("Testing"))
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

var app = builder.Build();

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

app.Run();
