using Application.Bookings.Commands;
using FluentValidation;

namespace Application.Bookings.Validators;

public class CreateBookingCommandValidator : AbstractValidator<CreateBookingCommand>
{
    public CreateBookingCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.CoachId).NotEmpty();
        RuleFor(x => x.StartAt).LessThan(x => x.EndAt);
        RuleFor(x => x.Status).IsInEnum();
        RuleFor(x => x.DurationMinutes)
            .GreaterThan(0)
            .When(x => x.DurationMinutes.HasValue);
    }
}
