using Application.CoachCertifications.Command;
using FluentValidation;

namespace Application.CoachCertifications.Validator;

public sealed class InactivateCoachCertificationCommandValidator : AbstractValidator<InactivateCoachCertificationCommand>
{
    public InactivateCoachCertificationCommandValidator()
    {
        RuleFor(x => x.CertificationId).NotEmpty();
    }
}
