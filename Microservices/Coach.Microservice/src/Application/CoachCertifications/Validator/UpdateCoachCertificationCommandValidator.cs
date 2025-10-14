using System;
using Application.CoachCertifications.Command;
using FluentValidation;

namespace Application.CoachCertifications.Validator;

public sealed class UpdateCoachCertificationCommandValidator : AbstractValidator<UpdateCoachCertificationCommand>
{
    public UpdateCoachCertificationCommandValidator()
    {
        RuleFor(x => x.CertificationId).NotEmpty();
        RuleFor(x => x.CertName).MaximumLength(255).When(x => x.CertName is not null);
        RuleFor(x => x.Issuer).MaximumLength(255).When(x => x.Issuer is not null);
        RuleFor(x => x.FileUrl)
            .Must(url => string.IsNullOrWhiteSpace(url) || Uri.TryCreate(url, UriKind.Absolute, out _))
            .WithMessage("FileUrl must be a valid absolute URI when provided.");
        RuleFor(x => x.ExpiresOn)
            .Must((command, expiresOn) => expiresOn is null || command.IssuedOn is null || expiresOn >= command.IssuedOn)
            .WithMessage("ExpiresOn must be after IssuedOn when both are provided.");
        When(x => x.File is not null, () =>
        {
            RuleFor(x => x.File!.Content)
                .Must(content => content is { Length: > 0 })
                .WithMessage("File content must not be empty.");
            RuleFor(x => x.File!.FileName).NotEmpty();
            RuleFor(x => x.File!.ContentType).NotEmpty();
        });
    }
}
