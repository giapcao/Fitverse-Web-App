using Application.Reviews.Query;
using FluentValidation;

namespace Application.Reviews.Validator;

public sealed class GetReviewByBookingIdQueryValidator : AbstractValidator<GetReviewByBookingIdQuery>
{
    public GetReviewByBookingIdQueryValidator()
    {
        RuleFor(query => query.BookingId).NotEmpty();
    }
}

