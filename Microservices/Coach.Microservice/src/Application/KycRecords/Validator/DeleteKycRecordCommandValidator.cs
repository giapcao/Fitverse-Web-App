using Application.KycRecords.Command;
using FluentValidation;

namespace Application.KycRecords.Validator;

public sealed class DeleteKycRecordCommandValidator : AbstractValidator<DeleteKycRecordCommand>
{
    public DeleteKycRecordCommandValidator()
    {
        RuleFor(x => x.RecordId).NotEmpty();
    }
}
