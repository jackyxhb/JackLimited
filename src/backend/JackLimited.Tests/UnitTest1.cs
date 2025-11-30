using FluentAssertions;
using JackLimited.Domain;

namespace JackLimited.Tests;

public class NpsCalculatorTests
{
    [Theory]
    [InlineData(new[] { 9, 10, 9 }, 100.0)]
    [InlineData(new[] { 0, 6, 0 }, -100.0)]
    [InlineData(new[] { 7, 8, 7 }, 0.0)]
    [InlineData(new[] { 9, 8, 6 }, 0.0)]
    [InlineData(new[] { 10, 9, 8, 7, 6 }, 20.0)]
    [InlineData(new[] { 5, 5, 5 }, -100.0)]
    [InlineData(new[] { 10, 10, 10 }, 100.0)]
    [InlineData(new[] { 7, 7, 7 }, 0.0)]
    [InlineData(new[] { 9, 7, 6 }, 0.0)]
    [InlineData(new[] { 10, 8, 6, 4 }, -25.0)]
    public void CalculateNps_ReturnsCorrectScore(int[] ratings, double expected)
    {
        // Act
        var result = NpsCalculator.CalculateNps(ratings);

        // Assert
        result.Should().Be(expected);
    }

    [Fact]
    public void CalculateNps_WithEmptyCollection_ReturnsZero()
    {
        // Arrange
        var ratings = Array.Empty<int>();

        // Act
        var result = NpsCalculator.CalculateNps(ratings);

        // Assert
        result.Should().Be(0.0);
    }

    [Fact]
    public void CalculateNps_WithSinglePromoter_Returns100()
    {
        // Arrange
        var ratings = new[] { 9 };

        // Act
        var result = NpsCalculator.CalculateNps(ratings);

        // Assert
        result.Should().Be(100.0);
    }

    [Fact]
    public void CalculateNps_WithSingleDetractor_ReturnsNegative100()
    {
        // Arrange
        var ratings = new[] { 6 };

        // Act
        var result = NpsCalculator.CalculateNps(ratings);

        // Assert
        result.Should().Be(-100.0);
    }

    [Fact]
    public void CalculateNps_WithSinglePassive_ReturnsZero()
    {
        // Arrange
        var ratings = new[] { 7 };

        // Act
        var result = NpsCalculator.CalculateNps(ratings);

        // Assert
        result.Should().Be(0.0);
    }
}
