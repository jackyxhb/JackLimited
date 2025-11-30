using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using JackLimited.Application;
using JackLimited.Infrastructure;

namespace JackLimited.Tests.IntegrationTests;

public record NpsResponse(double Nps);
public record AverageResponse(double Average);

public class SurveySubmissionTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public SurveySubmissionTests(WebApplicationFactory<Program> factory)
    {
        Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Testing");
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                // Replace the DbContext with a unique in-memory database for each test
                var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<JackLimitedDbContext>));
                if (descriptor != null)
                {
                    services.Remove(descriptor);
                }
                var databaseName = $"TestDb_{Guid.NewGuid()}";
                services.AddDbContext<JackLimitedDbContext>(options =>
                    options.UseInMemoryDatabase(databaseName));
            });
        });
    }

    [Fact]
    public async Task SubmitSurvey_WithValidData_ReturnsCreatedWithLocation()
    {
        // Arrange
        var client = _factory.CreateClient();
        var request = new
        {
            LikelihoodToRecommend = 9,
            Comments = "Great service!",
            Email = "test@example.com"
        };

        // Act
        var response = await client.PostAsJsonAsync("/api/survey", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        response.Headers.Location.Should().NotBeNull();
        // Data is saved to the database (verified by EF Core logging)
    }

    [Fact]
    public async Task SubmitSurvey_WithMinimumValidData_ReturnsCreated()
    {
        // Arrange
        var client = _factory.CreateClient();
        var request = new
        {
            LikelihoodToRecommend = 5,
            Comments = (string?)null,
            Email = (string?)null
        };

        // Act
        var response = await client.PostAsJsonAsync("/api/survey", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(11)]
    public async Task SubmitSurvey_WithInvalidLikelihoodToRecommend_ReturnsBadRequest(int invalidRating)
    {
        // Arrange
        var client = _factory.CreateClient();
        var request = new
        {
            LikelihoodToRecommend = invalidRating,
            Comments = "Test comment",
            Email = "test@example.com"
        };

        // Act
        var response = await client.PostAsJsonAsync("/api/survey", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var problemDetails = await response.Content.ReadFromJsonAsync<Microsoft.AspNetCore.Mvc.ProblemDetails>();
        problemDetails.Should().NotBeNull();
        problemDetails!.Title.Should().Be("One or more validation errors occurred.");
    }

    [Fact]
    public async Task SubmitSurvey_WithCommentsTooLong_ReturnsBadRequest()
    {
        // Arrange
        var client = _factory.CreateClient();
        var longComment = new string('x', 1001);
        var request = new
        {
            LikelihoodToRecommend = 8,
            Comments = longComment,
            Email = "test@example.com"
        };

        // Act
        var response = await client.PostAsJsonAsync("/api/survey", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task SubmitSurvey_WithUnsafeComments_ReturnsBadRequest()
    {
        // Arrange
        var client = _factory.CreateClient();
        var request = new
        {
            LikelihoodToRecommend = 8,
            Comments = "<script>alert('xss')</script>",
            Email = "test@example.com"
        };

        // Act
        var response = await client.PostAsJsonAsync("/api/survey", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Theory]
    [InlineData("invalid-email")]
    [InlineData("test@")]
    [InlineData("@example.com")]
    public async Task SubmitSurvey_WithInvalidEmail_ReturnsBadRequest(string invalidEmail)
    {
        // Arrange
        var client = _factory.CreateClient();
        var request = new
        {
            LikelihoodToRecommend = 8,
            Comments = "Test comment",
            Email = invalidEmail
        };

        // Act
        var response = await client.PostAsJsonAsync("/api/survey", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetNps_WithNoSurveys_ReturnsZero()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act
        var response = await client.GetAsync("/api/survey/nps");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<NpsResponse>();
        result!.Nps.Should().Be(0);
    }

    [Fact]
    public async Task GetNps_WithSurveys_ReturnsCorrectNps()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Submit some test data
        var surveys = new[]
        {
            new { LikelihoodToRecommend = 10, Comments = (string?)null, Email = (string?)null },
            new { LikelihoodToRecommend = 9, Comments = (string?)null, Email = (string?)null },
            new { LikelihoodToRecommend = 8, Comments = (string?)null, Email = (string?)null },
            new { LikelihoodToRecommend = 7, Comments = (string?)null, Email = (string?)null },
            new { LikelihoodToRecommend = 6, Comments = (string?)null, Email = (string?)null }
        };

        foreach (var survey in surveys)
        {
            await client.PostAsJsonAsync("/api/survey", survey);
        }

        // Act
        var response = await client.GetAsync("/api/survey/nps");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<NpsResponse>();
        result!.Nps.Should().Be(20.0); // (2 promoters - 1 detractor) / 5 * 100 = 20
    }

    [Fact]
    public async Task GetAverage_WithNoSurveys_ReturnsZero()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act
        var response = await client.GetAsync("/api/survey/average");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<AverageResponse>();
        result!.Average.Should().Be(0);
    }

    [Fact]
    public async Task GetAverage_WithSurveys_ReturnsCorrectAverage()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Submit test data
        var surveys = new[]
        {
            new { LikelihoodToRecommend = 8, Comments = (string?)null, Email = (string?)null },
            new { LikelihoodToRecommend = 6, Comments = (string?)null, Email = (string?)null },
            new { LikelihoodToRecommend = 10, Comments = (string?)null, Email = (string?)null }
        };

        foreach (var survey in surveys)
        {
            await client.PostAsJsonAsync("/api/survey", survey);
        }

        // Act
        var response = await client.GetAsync("/api/survey/average");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<AverageResponse>();
        result!.Average.Should().Be(8.0); // (8 + 6 + 10) / 3 = 8
    }

    [Fact]
    public async Task GetDistribution_WithNoSurveys_ReturnsEmptyDictionary()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act
        var response = await client.GetAsync("/api/survey/distribution");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<Dictionary<int, int>>();
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetDistribution_WithSurveys_ReturnsCorrectDistribution()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Submit test data with repeated ratings
        var surveys = new[]
        {
            new { LikelihoodToRecommend = 10, Comments = (string?)null, Email = (string?)null },
            new { LikelihoodToRecommend = 8, Comments = (string?)null, Email = (string?)null },
            new { LikelihoodToRecommend = 8, Comments = (string?)null, Email = (string?)null },
            new { LikelihoodToRecommend = 6, Comments = (string?)null, Email = (string?)null }
        };

        foreach (var survey in surveys)
        {
            await client.PostAsJsonAsync("/api/survey", survey);
        }

        // Act
        var response = await client.GetAsync("/api/survey/distribution");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<Dictionary<int, int>>();
        result.Should().NotBeNull();
        result![10].Should().Be(1);
        result[8].Should().Be(2);
        result[6].Should().Be(1);
    }
}