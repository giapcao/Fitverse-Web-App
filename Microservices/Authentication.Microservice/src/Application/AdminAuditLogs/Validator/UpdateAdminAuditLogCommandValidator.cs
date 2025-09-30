using Application.AdminAuditLogs.Command;
using FluentValidation;

namespace Application.AdminAuditLogs.Validator;

public sealed class UpdateAdminAuditLogCommandValidator : AbstractValidator<UpdateAdminAuditLogCommand>
{
    public UpdateAdminAuditLogCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty();

        When(x => !string.IsNullOrWhiteSpace(x.Action), () =>
        {
            RuleFor(x => x.Action!)
                .MaximumLength(256);
        });

        When(x => !string.IsNullOrWhiteSpace(x.TargetType), () =>
        {
            RuleFor(x => x.TargetType!)
                .MaximumLength(128);
        });
    }
}
