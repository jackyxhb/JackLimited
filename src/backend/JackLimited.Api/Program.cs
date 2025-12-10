using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using JackLimited.Api.Filters;
using JackLimited.Api.Observability;
using JackLimited.Application;
using JackLimited.Domain;
using JackLimited.Infrastructure;
using Microsoft.EntityFrameworkCore;
using System.Runtime.CompilerServices;
using AspNetCoreRateLimit;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using OpenTelemetry.Instrumentation.EntityFrameworkCore;
using OpenTelemetry.Logs;
using System.Diagnostics;
using System.Text.Json;
using System.Diagnostics.Metrics;
using System.Reflection;
using System.Linq;

[assembly: InternalsVisibleTo("JackLimited.Tests")]

var builder = WebApplication.CreateBuilder(args);
var testingApiKey = builder.Configuration["Testing:ApiKey"] ?? "local-testing-key";
const string TestAuthHeaderName = "X-Test-Auth";
const string CorrelationIdHeaderName = "X-Correlation-ID";

Activity.DefaultIdFormat = ActivityIdFormat.W3C;
Activity.ForceDefaultIdFormat = true;

// Add user secrets in development
if (builder.Environment.IsEnvironment("Development"))
{
    builder.Configuration.AddUserSecrets<Program>();
}

builder.Services.AddValidatorsFromAssembly(typeof(SurveyRequest).Assembly);
builder.Services.AddProblemDetails();
// Add services
builder.Services.AddValidatorsFromAssembly(typeof(SurveyRequest).Assembly);
// builder.Services.AddEndpointsApiExplorer();
// builder.Services.AddSwaggerGen(options =>
// {
//     options.SwaggerDoc("v1", new OpenApiInfo
//     {
//         Title = "JackLimited API",
//         Version = "v1",
//         Description = "Endpoints for submitting surveys and retrieving NPS analytics."
//     });
// });

// Configure rate limiting
// builder.Services.AddMemoryCache();
// builder.Services.Configure<IpRateLimitOptions>(builder.Configuration.GetSection("IpRateLimiting"));
// builder.Services.AddSingleton<IIpPolicyStore, MemoryCacheIpPolicyStore>();
// builder.Services.AddSingleton<IRateLimitCounterStore, MemoryCacheRateLimitCounterStore>();
// builder.Services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();
// builder.Services.AddSingleton<IProcessingStrategy, AsyncKeyLockProcessingStrategy>();

if (builder.Environment.IsEnvironment("Testing"))
{
    builder.Services.AddDbContext<JackLimitedDbContext>(options =>
        options.UseInMemoryDatabase("TestDb"));
}
else
{
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    if (string.IsNullOrWhiteSpace(connectionString))
    {
        throw new InvalidOperationException("Database connection string 'DefaultConnection' was not found.");
    }

    builder.Services.AddDbContext<JackLimitedDbContext>(options =>
        options.UseNpgsql(connectionString));
}

builder.Services.AddScoped<ISurveyRepository, SurveyRepository>();
var healthChecks = builder.Services.AddHealthChecks();
healthChecks.AddDbContextCheck<JackLimitedDbContext>("database", tags: new[] { "ready" });

var otlpEndpoint = builder.Configuration["OpenTelemetry:OtlpEndpoint"];
var serviceName = builder.Environment.ApplicationName ?? "JackLimited.Api";
var serviceVersion = Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "1.0.0";
var environmentName = builder.Environment.EnvironmentName;

// var openTelemetryBuilder = builder.Services
//     .AddOpenTelemetry()
//     .ConfigureResource(resource =>
//     {
//         resource.AddService(
//             serviceName: serviceName,
//             serviceVersion: serviceVersion,
//             serviceInstanceId: Environment.MachineName);
//         resource.AddAttributes(new[]
//         {
//             new KeyValuePair<string, object>("deployment.environment", environmentName)
//         });
//     });

// openTelemetryBuilder.WithTracing(tracing =>
// {
//     tracing
//         .AddAspNetCoreInstrumentation()
//         .AddHttpClientInstrumentation()
//         .AddEntityFrameworkCoreInstrumentation()
//         .AddSource("JackLimited.Api");

//     if (!string.IsNullOrWhiteSpace(otlpEndpoint) && Uri.TryCreate(otlpEndpoint, UriKind.Absolute, out var traceEndpoint))
//     {
//         tracing.AddOtlpExporter(options => options.Endpoint = traceEndpoint);
//     }
// });

// openTelemetryBuilder.WithMetrics(metrics =>
// {
//     metrics
//         .AddAspNetCoreInstrumentation()
//         .AddHttpClientInstrumentation()
//         .AddRuntimeInstrumentation()
//         .AddMeter("JackLimited.Api");

//     if (!string.IsNullOrWhiteSpace(otlpEndpoint) && Uri.TryCreate(otlpEndpoint, UriKind.Absolute, out var metricEndpoint))
//     {
//         metrics.AddOtlpExporter(options => options.Endpoint = metricEndpoint);
//     }
// });

// builder.Logging.AddOpenTelemetry(logging =>
// {
//     logging.IncludeFormattedMessage = true;
//     logging.IncludeScopes = true;
//     logging.ParseStateValues = true;
//     logging.SetResourceBuilder(
//         ResourceBuilder.CreateDefault()
//             .AddService(
//                 serviceName: serviceName,
//                 serviceVersion: serviceVersion,
//                 serviceInstanceId: Environment.MachineName)
//             .AddAttributes(new[]
//             {
//                 new KeyValuePair<string, object>("deployment.environment", environmentName)
//             }));

//     if (!string.IsNullOrWhiteSpace(otlpEndpoint) && Uri.TryCreate(otlpEndpoint, UriKind.Absolute, out var logEndpoint))
//     {
//         logging.AddOtlpExporter(options => options.Endpoint = logEndpoint);
//     }
// });

builder.Services.AddSingleton<ActivitySource>(_ => new ActivitySource("JackLimited.Api"));
builder.Services.AddSingleton<SurveyMetrics>();

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins("http://localhost:5173", "http://localhost:5174", "https://jacksurvey-webapp.azurewebsites.net") // Vite dev server + production
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

// Run database migrations in production
// if (!app.Environment.IsDevelopment() && !app.Environment.IsEnvironment("Testing"))
// {
//     using (var scope = app.Services.CreateScope())
//     {
//         var context = scope.ServiceProvider.GetRequiredService<JackLimitedDbContext>();
//         context.Database.Migrate();
//     }
// }

// Configure forwarded headers (important for proxies like Azure App Service)
app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = Microsoft.AspNetCore.HttpOverrides.ForwardedHeaders.XForwardedFor | Microsoft.AspNetCore.HttpOverrides.ForwardedHeaders.XForwardedProto
});

app.UseExceptionHandler();

app.Use(async (context, next) =>
{
    var correlationId = context.Request.Headers[CorrelationIdHeaderName].FirstOrDefault();
    if (string.IsNullOrWhiteSpace(correlationId))
    {
        correlationId = Activity.Current?.TraceId.ToString() ?? Guid.NewGuid().ToString("N");
        context.Request.Headers[CorrelationIdHeaderName] = correlationId;
    }

    context.TraceIdentifier = correlationId;
    context.Response.Headers[CorrelationIdHeaderName] = correlationId;

    await next();
});

// Configure the HTTP request pipeline
if (!app.Environment.IsDevelopment())
{
    // Enable HSTS in production
    app.UseHsts();
}
if (app.Environment.IsDevelopment() || app.Environment.IsEnvironment("Testing"))
{
    // Surface OpenAPI docs for local/test automation only
    // app.UseSwagger();
    // app.UseSwaggerUI(options =>
    // {
    //     options.SwaggerEndpoint("/swagger/v1/swagger.json", "JackLimited API v1");
    //     options.RoutePrefix = "swagger";
    // });
}

// Use rate limiting
// app.UseIpRateLimiting();

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
// app.UseDefaultFiles();
// app.UseStaticFiles();

// Use CORS
app.UseCors();

app.MapHealthChecks("/healthz", new HealthCheckOptions
{
    ResponseWriter = WriteHealthCheckResponse
});

app.MapHealthChecks("/readyz", new HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("ready"),
    ResponseWriter = WriteHealthCheckResponse
});

// app.MapFallbackToFile("index.html");

// Minimal API endpoint with enhanced observability
app.MapPost("/api/survey", async (
    SurveyRequest request,
    ISurveyRepository repository,
    SurveyMetrics metrics,
    ActivitySource activitySource,
    ILogger<Program> logger) =>
{
    var stopwatch = Stopwatch.StartNew();
    using var activity = activitySource.StartActivity("SubmitSurvey");

    try
    {
        var sanitizedComments = string.IsNullOrWhiteSpace(request.Comments)
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

        stopwatch.Stop();
        metrics.RecordSubmission(stopwatch.Elapsed, SurveyMetrics.SubmissionOutcome.Success);
        activity?.SetStatus(ActivityStatusCode.Ok);

        return Results.Created($"/api/survey/{survey.Id}", new { Message = "Survey submitted successfully", Id = survey.Id });
    }
    catch (Exception ex)
    {
        stopwatch.Stop();
        metrics.RecordSubmission(stopwatch.Elapsed, SurveyMetrics.SubmissionOutcome.Failure);
        activity?.SetStatus(ActivityStatusCode.Error, "SurveySubmissionFailure");
        logger.LogError(ex, "Failed to submit survey");

        return Results.Problem(
            title: "Submission Failed",
            detail: "We were unable to save your feedback. Please try again.");
    }
}).AddEndpointFilter<ValidationFilter<SurveyRequest>>();

app.MapGet("/api/test", () => 
{
    Console.WriteLine("GET /api/test called");
    return Results.Ok("Test endpoint works");
});

app.MapGet("/api/survey/nps", async (
    ISurveyRepository repository,
    SurveyMetrics metrics,
    ActivitySource activitySource,
    ILogger<Program> logger) =>
{
    var stopwatch = Stopwatch.StartNew();
    using var activity = activitySource.StartActivity("GetSurveyNps");

    try
    {
        var ratings = await repository.GetAllRatingsAsync();
        var npsScore = NpsCalculator.CalculateNps(ratings);

        stopwatch.Stop();
        metrics.RecordAnalytics(stopwatch.Elapsed, SurveyMetrics.AnalyticsOutcome.Success, "/api/survey/nps");
        activity?.SetStatus(ActivityStatusCode.Ok);

        return Results.Ok(new { Nps = npsScore });
    }
    catch (Exception ex)
    {
        stopwatch.Stop();
        metrics.RecordAnalytics(stopwatch.Elapsed, SurveyMetrics.AnalyticsOutcome.Failure, "/api/survey/nps");
        activity?.SetStatus(ActivityStatusCode.Error, "AnalyticsFailure");
        logger.LogError(ex, "Failed to calculate NPS");

        return Results.Problem(
            title: "Data Retrieval Error",
            detail: "Failed to calculate Net Promoter Score. Please try again later.",
            statusCode: 500
        );
    }
});

app.MapGet("/api/survey/average", async (
    ISurveyRepository repository,
    SurveyMetrics metrics,
    ActivitySource activitySource,
    ILogger<Program> logger) =>
{
    var stopwatch = Stopwatch.StartNew();
    using var activity = activitySource.StartActivity("GetSurveyAverage");

    try
    {
        var average = await repository.GetAverageRatingAsync();

        stopwatch.Stop();
        metrics.RecordAnalytics(stopwatch.Elapsed, SurveyMetrics.AnalyticsOutcome.Success, "/api/survey/average");
        activity?.SetStatus(ActivityStatusCode.Ok);

        return Results.Ok(new { Average = Math.Round(average, 2) });
    }
    catch (Exception ex)
    {
        stopwatch.Stop();
        metrics.RecordAnalytics(stopwatch.Elapsed, SurveyMetrics.AnalyticsOutcome.Failure, "/api/survey/average");
        activity?.SetStatus(ActivityStatusCode.Error, "AnalyticsFailure");
        logger.LogError(ex, "Failed to calculate average rating");
        return Results.Problem(
            title: "Data Retrieval Error",
            detail: "Failed to calculate average rating. Please try again later.",
            statusCode: 500
        );
    }
});

app.MapGet("/api/survey/distribution", async (
    ISurveyRepository repository,
    SurveyMetrics metrics,
    ActivitySource activitySource,
    ILogger<Program> logger) =>
{
    var stopwatch = Stopwatch.StartNew();
    using var activity = activitySource.StartActivity("GetSurveyDistribution");

    try
    {
        var ratings = await repository.GetAllRatingsAsync();
        var distribution = ratings
            .GroupBy(r => r)
            .ToDictionary(g => g.Key, g => g.Count());

        stopwatch.Stop();
        metrics.RecordAnalytics(stopwatch.Elapsed, SurveyMetrics.AnalyticsOutcome.Success, "/api/survey/distribution");
        activity?.SetStatus(ActivityStatusCode.Ok);

        return Results.Ok(distribution);
    }
    catch (Exception ex)
    {
        stopwatch.Stop();
        metrics.RecordAnalytics(stopwatch.Elapsed, SurveyMetrics.AnalyticsOutcome.Failure, "/api/survey/distribution");
        activity?.SetStatus(ActivityStatusCode.Error, "AnalyticsFailure");
        logger.LogError(ex, "Failed to retrieve rating distribution");
        return Results.Problem(
            title: "Data Retrieval Error",
            detail: "Failed to retrieve rating distribution. Please try again later.",
            statusCode: 500
        );
    }
});

static Task WriteHealthCheckResponse(HttpContext context, HealthReport report)
{
    context.Response.ContentType = "application/json";

    var response = new
    {
        status = report.Status.ToString(),
        durationMs = report.TotalDuration.TotalMilliseconds,
        checks = report.Entries.Select(entry => new
        {
            name = entry.Key,
            status = entry.Value.Status.ToString(),
            durationMs = entry.Value.Duration.TotalMilliseconds,
            description = entry.Value.Description,
            tags = entry.Value.Tags
        })
    };

    var payload = JsonSerializer.Serialize(response, new JsonSerializerOptions(JsonSerializerDefaults.Web));
    return context.Response.WriteAsync(payload);
}

bool IsAuthorized(HttpContext context)
{
    if (!context.Request.Headers.TryGetValue(TestAuthHeaderName, out var provided))
    {
        return false;
    }

    return string.Equals(provided.ToString(), testingApiKey, StringComparison.Ordinal);
}

if (app.Environment.IsEnvironment("Testing"))
{
    app.MapPost("/testing/reset", async (HttpContext httpContext, JackLimitedDbContext dbContext) =>
    {
        if (!IsAuthorized(httpContext))
        {
            return Results.Unauthorized();
        }

        // Clear existing surveys so each test case starts from a blank slate
        dbContext.Surveys.RemoveRange(dbContext.Surveys);
        await dbContext.SaveChangesAsync();
        return Results.Ok(new { Message = "Test data cleared" });
    });

    app.MapPost("/testing/seed", async (HttpContext httpContext, IEnumerable<SurveyRequest> seeds, IValidator<SurveyRequest> validator, JackLimitedDbContext dbContext) =>
    {
        if (!IsAuthorized(httpContext))
        {
            return Results.Unauthorized();
        }

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

            // Sanitize inline to guarantee the testing helpers match production behavior
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
