using System;
using Application.CoachMedia.Command;
using FluentValidation;

namespace Application.CoachMedia.Validator;

public sealed class CreateCoachMediaCommandValidator : AbstractValidator<CreateCoachMediaCommand>
{
    public CreateCoachMediaCommandValidator()
    {
        RuleFor(x => x.CoachId).NotEmpty();
        RuleFor(x => x)
            .Must(cmd => cmd.File is not null || !string.IsNullOrWhiteSpace(cmd.Url))
            .WithMessage("Either a media file or a URL must be provided.");
        When(x => !string.IsNullOrWhiteSpace(x.Url), () =>
        {
            RuleFor(x => x.Url!)
                .Must(uri => Uri.TryCreate(uri, UriKind.Absolute, out _))
                .WithMessage("Url must be a valid absolute URI.");
        });
        When(x => x.File is not null, () =>
        {
            RuleFor(x => x.File!.Content)
                .Must(content => content is { Length: > 0 })
                .WithMessage("File content must not be empty.");
            RuleFor(x => x.File!.FileName).NotEmpty();
            RuleFor(x => x.File!.ContentType).NotEmpty();
        });
        RuleFor(x => x.MediaName).MaximumLength(255).When(x => x.MediaName is not null);
        RuleFor(x => x.Description).MaximumLength(2048).When(x => x.Description is not null);
    }
}
