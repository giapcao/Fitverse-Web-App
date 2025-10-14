using System;
using Application.CoachMedia.Command;
using FluentValidation;

namespace Application.CoachMedia.Validator;

public sealed class CreateCoachMediaCommandValidator : AbstractValidator<CreateCoachMediaCommand>
{
    public CreateCoachMediaCommandValidator()
    {
        RuleFor(x => x.CoachId).NotEmpty();
        RuleFor(x => x.Url)
            .NotEmpty()
            .Must(uri => Uri.TryCreate(uri, UriKind.Absolute, out _))
            .WithMessage("Url must be a valid absolute URI.");
        RuleFor(x => x.MediaName).MaximumLength(255).When(x => x.MediaName is not null);
        RuleFor(x => x.Description).MaximumLength(2048).When(x => x.Description is not null);
    }
}
