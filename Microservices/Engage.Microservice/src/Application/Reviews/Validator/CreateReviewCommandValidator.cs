using Application.Reviews.Command;
using FluentValidation;

namespace Application.Reviews.Validator;

public sealed class CreateReviewCommandValidator : AbstractValidator<CreateReviewCommand>
{
    public CreateReviewCommandValidator()
    {
        RuleFor(command => command.BookingId).NotEmpty();
        RuleFor(command => command.UserId).NotEmpty();
        RuleFor(command => command.CoachId).NotEmpty();
        RuleFor(command => command.Rating).InclusiveBetween(1, 5);
        RuleFor(command => command.Comment).MaximumLength(1000);
    }
}

