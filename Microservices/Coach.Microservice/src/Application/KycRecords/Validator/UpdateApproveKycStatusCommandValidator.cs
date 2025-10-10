using Application.KycRecords.Command;
using FluentValidation;

namespace Application.KycRecords.Validator;

public sealed class UpdateApproveKycStatusCommandValidator : AbstractValidator<UpdateApproveKycStatusCommand>
{
    public UpdateApproveKycStatusCommandValidator()
    {
        RuleFor(x => x.RecordId).NotEmpty();
        RuleFor(x => x.AdminNote).MaximumLength(2000).When(x => x.AdminNote is not null);
    }
}
