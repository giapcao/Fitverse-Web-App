using System;
using Application.Users.Command;
using FluentValidation;

namespace Application.Users.Validator;

public sealed class UpdateUserCommandValidator : AbstractValidator<UpdateUserCommand>
{
    public UpdateUserCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty();

        RuleFor(x => x.Email!)
            .EmailAddress()
            .When(x => !string.IsNullOrWhiteSpace(x.Email));

        RuleFor(x => x)
            .Must(HasAnyUpdatableField)
            .WithMessage("At least one field must be provided for update.");

        When(x => !string.IsNullOrWhiteSpace(x.FullName), () =>
        {
            RuleFor(x => x.FullName!)
                .MaximumLength(255);
        });

        When(x => !string.IsNullOrWhiteSpace(x.Phone), () =>
        {
            RuleFor(x => x.Phone!)
                .NotEmpty()
                .MaximumLength(32);
        });

        When(x => !string.IsNullOrWhiteSpace(x.AvatarUrl), () =>
        {
            RuleFor(x => x.AvatarUrl!)
                .Must(IsValidAbsoluteUri)
                .WithMessage("AvatarUrl must be a valid absolute URI.");
        });

        When(x => !string.IsNullOrWhiteSpace(x.Gender), () =>
        {
            RuleFor(x => x.Gender!)
                .NotEmpty()
                .MaximumLength(32);
        });

        When(x => x.Birth.HasValue, () =>
        {
            RuleFor(x => x.Birth!.Value)
                .LessThanOrEqualTo(_ => DateOnly.FromDateTime(DateTime.UtcNow))
                .WithMessage("Birth date cannot be in the future.");
        });

        When(x => x.Description is not null, () =>
        {
            RuleFor(x => x.Description!)
                .MaximumLength(1024);
        });

        When(x => x.HomeLat.HasValue, () =>
        {
            RuleFor(x => x.HomeLat!.Value)
                .InclusiveBetween(-90, 90);
        });

        When(x => x.HomeLng.HasValue, () =>
        {
            RuleFor(x => x.HomeLng!.Value)
                .InclusiveBetween(-180, 180);
        });
    }

    private static bool HasAnyUpdatableField(UpdateUserCommand command) =>
        !string.IsNullOrWhiteSpace(command.Email)
        || !string.IsNullOrWhiteSpace(command.FullName)
        || !string.IsNullOrWhiteSpace(command.Phone)
        || !string.IsNullOrWhiteSpace(command.AvatarUrl)
        || !string.IsNullOrWhiteSpace(command.Gender)
        || command.Birth.HasValue
        || command.Description is not null
        || command.HomeLat.HasValue
        || command.HomeLng.HasValue;

    private static bool IsValidAbsoluteUri(string value) =>
        Uri.TryCreate(value, UriKind.Absolute, out _);
}
