using Application.Conversations.Command;
using FluentValidation;

namespace Application.Conversations.Validator;

public sealed class SendMessageCommandValidator : AbstractValidator<SendMessageCommand>
{
    public SendMessageCommandValidator()
    {
        RuleFor(command => command.UserId).NotEmpty();
        RuleFor(command => command.CoachId).NotEmpty();
        RuleFor(command => command.SenderId).NotEmpty();
        RuleFor(command => command)
            .Must(command => !string.IsNullOrWhiteSpace(command.Body) || !string.IsNullOrWhiteSpace(command.AttachmentUrl))
            .WithMessage("Either body or attachment must be provided.");
    }
}

