using FluentAssertions;
using JackLimited.Domain;
using Xunit;

namespace JackLimited.Tests.UnitTests;

public class NpsCalculatorTests
{
    [Theory]
    [InlineData(new int[] { }, 0)]
    [InlineData(new[] { 9, 10, 10 }, 100)]
    [InlineData(new[] { 0, 6, 6 }, -100)]
    [InlineData(new[] { 7, 8, 9 }, 33.33)]
    [InlineData(new[] { 5, 5, 5, 10, 10, 10 }, 0)]
    [InlineData(new[] { 9, 7, 5 }, 0)]
    public void CalculateNps_WithVariousRatings_ReturnsCorrectNps(int[] ratings, double expectedNps)
    {
        // Act
        var nps = NpsCalculator.CalculateNps(ratings);

        // Assert
        nps.Should().Be(expectedNps);
    }
}