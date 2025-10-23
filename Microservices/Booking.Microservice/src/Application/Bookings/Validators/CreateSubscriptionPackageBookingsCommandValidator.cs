using System.Linq;
using Application.Bookings.Commands;
using FluentValidation;

namespace Application.Bookings.Validators;

public class CreateSubscriptionPackageBookingsCommandValidator : AbstractValidator<CreateSubscriptionPackageBookingsCommand>
{
    public CreateSubscriptionPackageBookingsCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.CoachId).NotEmpty();
        RuleFor(x => x.TimeslotIds)
            .NotNull()
            .Must(ids => ids.Count > 0)
            .WithMessage("At least one timeslot id must be provided.")
            .Must(ids => ids.Count == ids.Distinct().Count())
            .WithMessage("Timeslot ids must be unique.");
    }
}
