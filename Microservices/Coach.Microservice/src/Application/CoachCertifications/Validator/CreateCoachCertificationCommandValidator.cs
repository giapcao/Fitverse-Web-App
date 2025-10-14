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
        RuleFor(x => x.ExpiresOn)
            .Must((command, expiresOn) => expiresOn is null || command.IssuedOn is null || expiresOn >= command.IssuedOn)
            .WithMessage("ExpiresOn must be after IssuedOn when both are provided.");
        RuleFor(x => x.Directory)
            .MaximumLength(512)
            .When(x => x.Directory is not null);
        RuleFor(x => x.File)
            .Must(file => file is null || (file.Content.Length > 0 && !string.IsNullOrWhiteSpace(file.FileName) && !string.IsNullOrWhiteSpace(file.ContentType)))
            .WithMessage("Uploaded file must include content, file name, and content type.");
    }
}
