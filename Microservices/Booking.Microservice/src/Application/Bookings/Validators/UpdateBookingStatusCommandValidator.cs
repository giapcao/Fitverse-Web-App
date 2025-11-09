using Application.Bookings.Commands;
using Domain.Persistence.Enums;
using FluentValidation;

namespace Application.Bookings.Validators;

public sealed class UpdateBookingStatusCommandValidator : AbstractValidator<UpdateBookingStatusCommand>
{
    public UpdateBookingStatusCommandValidator()
    {
        RuleFor(x => x.BookingId).NotEmpty();
        RuleFor(x => x.RequestedStatus)
            .Must(status => status is BookingStatus.ConfirmedByCoach or BookingStatus.ConfirmedByUser)
            .WithMessage("RequestedStatus must be either 'ConfirmedByCoach' or 'ConfirmedByUser'.");
    }
}
