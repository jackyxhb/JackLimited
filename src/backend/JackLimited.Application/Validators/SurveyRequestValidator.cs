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

        // Check for dangerous content
        var dangerousPatterns = new[]
        {
            @"<script[^>]*>.*?</script>", // script tags
            @"<[^>]+>", // any HTML tags
            @"javascript:", // javascript URLs
            @"on\w+\s*=", // event handlers
            @"&", // HTML entities and ampersands
            @"[""']", // quotes and apostrophes
        };

        foreach (var pattern in dangerousPatterns)
        {
            if (Regex.IsMatch(text, pattern, RegexOptions.IgnoreCase))
            {
                return false;
            }
        }

        return true;
    }

    private bool BeValidEmailFormat(string? email)
    {
        if (string.IsNullOrEmpty(email)) return true;

        // More restrictive email regex - no consecutive dots, proper format
        var emailRegex = @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$";
        if (!Regex.IsMatch(email, emailRegex)) return false;

        // Additional checks
        var localPart = email.Split('@')[0];
        var domainPart = email.Split('@')[1];

        // No consecutive dots in local part
        if (localPart.Contains("..")) return false;

        // No consecutive dots in domain part
        if (domainPart.Contains("..")) return false;

        // Domain should have at least one dot
        if (!domainPart.Contains(".")) return false;

        return true;
    }
}