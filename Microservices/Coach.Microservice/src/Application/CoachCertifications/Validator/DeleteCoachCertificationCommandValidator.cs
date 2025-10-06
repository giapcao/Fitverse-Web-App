using Application.CoachCertifications.Command;
using FluentValidation;

namespace Application.CoachCertifications.Validator;

public sealed class DeleteCoachCertificationCommandValidator : AbstractValidator<DeleteCoachCertificationCommand>
{
    public DeleteCoachCertificationCommandValidator()
    {
        RuleFor(x => x.CertificationId).NotEmpty();
    }
}
