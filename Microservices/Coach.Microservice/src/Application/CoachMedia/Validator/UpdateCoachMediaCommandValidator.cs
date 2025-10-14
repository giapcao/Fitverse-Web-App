using System;
using Application.CoachMedia.Command;
using FluentValidation;

namespace Application.CoachMedia.Validator;

public sealed class UpdateCoachMediaCommandValidator : AbstractValidator<UpdateCoachMediaCommand>
{
    public UpdateCoachMediaCommandValidator()
    {
        RuleFor(x => x.MediaId).NotEmpty();
        RuleFor(x => x.Url)
            .Must(uri => uri is null || Uri.TryCreate(uri, UriKind.Absolute, out _))
            .WithMessage("Url must be a valid absolute URI when provided.");
        RuleFor(x => x.MediaName).MaximumLength(255).When(x => x.MediaName is not null);
        RuleFor(x => x.Description).MaximumLength(2048).When(x => x.Description is not null);
    }
}
