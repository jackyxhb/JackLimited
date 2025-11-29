using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace JackLimited.Tests.IntegrationTests;

public class SurveySubmissionTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public SurveySubmissionTests(WebApplicationFactory<Program> factory)
    {
        Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Testing");
        _factory = factory;
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
}