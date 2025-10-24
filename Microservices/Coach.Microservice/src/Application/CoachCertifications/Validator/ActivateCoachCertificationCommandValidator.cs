using Application.CoachCertifications.Command;
using FluentValidation;

namespace Application.CoachCertifications.Validator;

public sealed class ActivateCoachCertificationCommandValidator : AbstractValidator<ActivateCoachCertificationCommand>
{
    public ActivateCoachCertificationCommandValidator()
    {
        RuleFor(x => x.CertificationId).NotEmpty();
    }
}
