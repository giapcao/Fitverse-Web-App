using System;
using Application.KycRecords.Command;
using FluentValidation;

namespace Application.KycRecords.Validator;

public sealed class UpdateKycRecordStatusCommandValidator : AbstractValidator<UpdateKycRecordStatusCommand>
{
    public UpdateKycRecordStatusCommandValidator()
    {
        RuleFor(x => x.RecordId).NotEmpty();
        RuleFor(x => x.AdminNote).MaximumLength(2000).When(x => x.AdminNote is not null);
    }
}
