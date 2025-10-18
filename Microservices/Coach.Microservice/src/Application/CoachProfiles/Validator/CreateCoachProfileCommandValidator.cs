using System;
using System.Collections.Generic;
using Application.CoachProfiles.Command;
using FluentValidation;

namespace Application.CoachProfiles.Validator;

public sealed class CreateCoachProfileCommandValidator : AbstractValidator<CreateCoachProfileCommand>
{
    private static readonly HashSet<string> AllowedGenders = new(StringComparer.OrdinalIgnoreCase)
    {
        "male",
        "female",
        "other",
        "unspecified"
    };

    public CreateCoachProfileCommandValidator()
    {
        RuleFor(x => x.CoachId).NotEmpty();
        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress();
        RuleFor(x => x.YearsExperience).GreaterThanOrEqualTo(0).When(x => x.YearsExperience.HasValue);
        RuleFor(x => x.BasePriceVnd).GreaterThanOrEqualTo(0).When(x => x.BasePriceVnd.HasValue);
        RuleFor(x => x.ServiceRadiusKm).GreaterThan(0).When(x => x.ServiceRadiusKm.HasValue);
        RuleFor(x => x.WeightKg).InclusiveBetween(0m, 500m).When(x => x.WeightKg.HasValue);
        RuleFor(x => x.HeightCm).InclusiveBetween(0m, 300m).When(x => x.HeightCm.HasValue);
        RuleFor(x => x.Gender)
            .Must(BeValidGender)
            .WithMessage("Gender must be one of male, female, other, or unspecified.")
            .When(x => !string.IsNullOrWhiteSpace(x.Gender));
        RuleFor(x => x.BirthDate)
            .LessThanOrEqualTo(_ => DateOnly.FromDateTime(DateTime.UtcNow))
            .WithMessage("Birth date cannot be in the future.")
            .When(x => x.BirthDate.HasValue);
        RuleFor(x => x.CitizenIssueDate)
            .LessThanOrEqualTo(_ => DateOnly.FromDateTime(DateTime.UtcNow))
            .WithMessage("Citizen issue date cannot be in the future.")
            .When(x => x.CitizenIssueDate.HasValue);
        RuleFor(x => x.TaxCode)
            .Matches(@"^\d{10}(\d{3})?$")
            .WithMessage("Tax code must contain 10 or 13 digits.")
            .When(x => !string.IsNullOrWhiteSpace(x.TaxCode));
        RuleFor(x => x.CitizenId)
            .Matches(@"^\d{9}(\d{3})?$")
            .WithMessage("Citizen ID must contain 9 or 12 digits.")
            .When(x => !string.IsNullOrWhiteSpace(x.CitizenId));
    }

    private static bool BeValidGender(string gender)
    {
        var normalized = gender.Trim();
        return AllowedGenders.Contains(normalized);
    }
}
