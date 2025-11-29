using FluentValidation;

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
            .WithMessage("Comments must not exceed 1000 characters.");

        RuleFor(x => x.Email)
            .EmailAddress()
            .When(x => !string.IsNullOrEmpty(x.Email))
            .WithMessage("Email must be a valid email address.");
    }
}