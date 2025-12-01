using FluentAssertions;
using FluentValidation;
using JackLimited.Application.Validators;
using System.Collections.Generic;

namespace JackLimited.Tests.UnitTests;

public class SurveyRequestValidatorTests
{
    private readonly SurveyRequestValidator _validator = new();

    public static IEnumerable<object?[]> ValidCommentCases => new List<object?[]>
    {
        new object?[] { null },
        new object?[] { string.Empty },
        new object?[] { "Valid comment" },
        new object?[] { new string('x', 1000) },
        new object?[] { "Quote friendly \"text\" & partners" }
    };

    [Theory]
    [InlineData(0)]
    [InlineData(5)]
    [InlineData(10)]
    public void Validate_LikelihoodToRecommend_ValidValues_ShouldNotHaveValidationError(int value)
    {
        // Arrange
        var request = new JackLimited.Application.SurveyRequest(value, null, null);

        // Act
        var result = _validator.Validate(request);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().NotContain(e => e.PropertyName == "LikelihoodToRecommend");
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(11)]
    [InlineData(15)]
    public void Validate_LikelihoodToRecommend_InvalidValues_ShouldHaveValidationError(int value)
    {
        // Arrange
        var request = new JackLimited.Application.SurveyRequest(value, null, null);

        // Act
        var result = _validator.Validate(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e =>
            e.PropertyName == "LikelihoodToRecommend" &&
            e.ErrorMessage == "Likelihood to recommend must be between 0 and 10.");
    }

    [Theory]
    [MemberData(nameof(ValidCommentCases))]
    public void Validate_Comments_ValidValues_ShouldNotHaveValidationError(string? comments)
    {
        // Arrange
        var request = new JackLimited.Application.SurveyRequest(5, comments, null);

        // Act
        var result = _validator.Validate(request);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().NotContain(e => e.PropertyName == "Comments");
    }

    [Fact]
    public void Validate_Comments_ExceedsMaxLength_ShouldHaveValidationError()
    {
        // Arrange
        var longComment = new string('x', 1001);
        var request = new JackLimited.Application.SurveyRequest(5, longComment, null);

        // Act
        var result = _validator.Validate(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e =>
            e.PropertyName == "Comments" &&
            e.ErrorMessage == "Comments must not exceed 1000 characters.");
    }

    [Theory]
    [InlineData("<script>alert('xss')</script>")]
    [InlineData("Comment with <b>html</b> tags")]
    [InlineData("javascript:alert('xss')")]
    [InlineData("onload=alert('xss')")]
    public void Validate_Comments_UnsafeContent_ShouldHaveValidationError(string unsafeComment)
    {
        // Arrange
        var request = new JackLimited.Application.SurveyRequest(5, unsafeComment, null);

        // Act
        var result = _validator.Validate(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e =>
            e.PropertyName == "Comments" &&
            e.ErrorMessage == "Comments contain invalid characters.");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("test@example.com")]
    [InlineData("user.name+tag@example.co.uk")]
    [InlineData("test.email@subdomain.example.com")]
    public void Validate_Email_ValidValues_ShouldNotHaveValidationError(string? email)
    {
        // Arrange
        var request = new JackLimited.Application.SurveyRequest(5, null, email);

        // Act
        var result = _validator.Validate(request);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().NotContain(e => e.PropertyName == "Email");
    }

    [Theory]
    [InlineData("invalid-email")]
    [InlineData("test@")]
    [InlineData("@example.com")]
    [InlineData("test..email@example.com")]
    [InlineData("test@example")]
    public void Validate_Email_InvalidFormat_ShouldHaveValidationError(string invalidEmail)
    {
        // Arrange
        var request = new JackLimited.Application.SurveyRequest(5, null, invalidEmail);

        // Act
        var result = _validator.Validate(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e =>
            e.PropertyName == "Email" &&
            e.ErrorMessage == "Email format is invalid.");
    }

    [Fact]
    public void Validate_Email_ExceedsMaxLength_ShouldHaveValidationError()
    {
        // Arrange
        var longEmail = "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa@example.com"; // 256+ characters
        var request = new JackLimited.Application.SurveyRequest(5, null, longEmail);

        // Act
        var result = _validator.Validate(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e =>
            e.PropertyName == "Email" &&
            e.ErrorMessage == "Email must not exceed 255 characters.");
    }

    [Fact]
    public void Validate_Email_InvalidEmailAddress_ShouldHaveValidationError()
    {
        // Arrange
        var request = new JackLimited.Application.SurveyRequest(5, null, "not-an-email");

        // Act
        var result = _validator.Validate(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e =>
            e.PropertyName == "Email" &&
            e.ErrorMessage == "Email must be a valid email address.");
    }
}