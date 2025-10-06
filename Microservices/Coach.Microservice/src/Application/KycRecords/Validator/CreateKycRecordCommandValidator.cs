using System;
using Application.KycRecords.Command;
using FluentValidation;

namespace Application.KycRecords.Validator;

public sealed class CreateKycRecordCommandValidator : AbstractValidator<CreateKycRecordCommand>
{
    public CreateKycRecordCommandValidator()
    {
        RuleFor(x => x.CoachId).NotEmpty();
        RuleFor(x => x.IdDocumentUrl)
            .Must(url => url is null || Uri.TryCreate(url, UriKind.Absolute, out _))
            .WithMessage("IdDocumentUrl must be a valid absolute URI when provided.");
        RuleFor(x => x.AdminNote).MaximumLength(2000).When(x => x.AdminNote is not null);
    }
}
