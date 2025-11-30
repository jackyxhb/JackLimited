using FluentValidation;
using System.Text.RegularExpressions;

namespace JackLimited.Application.Validators;

public class SurveyRequestValidator : AbstractValidator<SurveyRequest>
{
    public SurveyRequestValidator()
    {
        RuleFor(x => x.LikelihoodToRecommend)
            .InclusiveBetween(0, 10)
            .WithMessage("Likelihood to recommend must be between 0 and 10.");

        RuleFor(x => x.Comments)
            .MaximumLength(1000)
            .WithMessage("Comments must not exceed 1000 characters.")
            .Must(BeSafeText)
            .WithMessage("Comments contain invalid characters.")
            .When(x => !string.IsNullOrEmpty(x.Comments));

        RuleFor(x => x.Email)
            .EmailAddress()
            .When(x => !string.IsNullOrEmpty(x.Email))
            .WithMessage("Email must be a valid email address.")
            .MaximumLength(255)
            .WithMessage("Email must not exceed 255 characters.")
            .Must(BeValidEmailFormat)
            .WithMessage("Email format is invalid.")
            .When(x => !string.IsNullOrEmpty(x.Email));
    }

    private bool BeSafeText(string? text)
    {
        if (string.IsNullOrEmpty(text)) return true;

        // Remove potential XSS vectors and control characters
        var sanitized = Regex.Replace(text, @"[<>\""'&]", "");
        sanitized = Regex.Replace(sanitized, @"[\x00-\x1F\x7F-\x9F]", "");

        // Check if the text changed significantly (indicating malicious content)
        return sanitized.Length >= text.Length * 0.8;
    }

    private bool BeValidEmailFormat(string? email)
    {
        if (string.IsNullOrEmpty(email)) return true;

        // Basic email regex - more restrictive than built-in EmailAddress validator
        var emailRegex = @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$";
        return Regex.IsMatch(email, emailRegex);
    }
}