using Application.KycRecords.Command;
using Domain.Persistence.Enums;
using FluentValidation;

namespace Application.KycRecords.Validator;

public sealed class UpdateKycRecordStatusCommandValidator : AbstractValidator<UpdateKycRecordStatusCommand>
{
    public UpdateKycRecordStatusCommandValidator()
    {
        RuleFor(x => x.RecordId).NotEmpty();
        RuleFor(x => x.AdminNote).MaximumLength(2000).When(x => x.AdminNote is not null);
        RuleFor(x => x.Status)
            .Must(status => status == KycStatus.Approved || status == KycStatus.Rejected)
            .WithMessage("KYC status must be Approved or Rejected.");
    }
}
