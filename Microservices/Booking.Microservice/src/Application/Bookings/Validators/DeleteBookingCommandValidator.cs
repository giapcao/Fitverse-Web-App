using Application.Bookings.Commands;
using FluentValidation;

namespace Application.Bookings.Validators;

public class DeleteBookingCommandValidator : AbstractValidator<DeleteBookingCommand>
{
    public DeleteBookingCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
    }
}
