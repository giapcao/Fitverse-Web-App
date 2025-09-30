using Application.AdminAuditLogs.Command;
using FluentValidation;

namespace Application.AdminAuditLogs.Validator;

public sealed class CreateAdminAuditLogCommandValidator : AbstractValidator<CreateAdminAuditLogCommand>
{
    public CreateAdminAuditLogCommandValidator()
    {
        RuleFor(x => x.AdminId)
            .NotEmpty();

        RuleFor(x => x.Action)
            .NotEmpty()
            .MaximumLength(256);

        RuleFor(x => x.TargetType)
            .NotEmpty()
            .MaximumLength(128);
    }
}
