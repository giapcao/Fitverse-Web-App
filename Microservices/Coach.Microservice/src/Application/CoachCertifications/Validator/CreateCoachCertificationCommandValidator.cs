using System;
using Application.CoachCertifications.Command;
using FluentValidation;

namespace Application.CoachCertifications.Validator;

public sealed class CreateCoachCertificationCommandValidator : AbstractValidator<CreateCoachCertificationCommand>
{
    public CreateCoachCertificationCommandValidator()
    {
        RuleFor(x => x.CoachId).NotEmpty();
        RuleFor(x => x.CertName).NotEmpty().MaximumLength(255);
        RuleFor(x => x.Issuer).MaximumLength(255).When(x => x.Issuer is not null);
        RuleFor(x => x.FileUrl)
            .Must(url => string.IsNullOrWhiteSpace(url) || Uri.TryCreate(url, UriKind.Absolute, out _))
            .WithMessage("FileUrl must be a valid absolute URI when provided.");
        RuleFor(x => x.Status).NotEmpty().MaximumLength(100);
        RuleFor(x => x.ExpiresOn)
            .Must((command, expiresOn) => expiresOn is null || command.IssuedOn is null || expiresOn >= command.IssuedOn)
            .WithMessage("ExpiresOn must be after IssuedOn when both are provided.");
    }
}
