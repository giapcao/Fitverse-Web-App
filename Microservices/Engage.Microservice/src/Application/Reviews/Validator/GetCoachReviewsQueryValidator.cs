using Application.Reviews.Query;
using FluentValidation;

namespace Application.Reviews.Validator;

public sealed class GetCoachReviewsQueryValidator : AbstractValidator<GetCoachReviewsQuery>
{
    public GetCoachReviewsQueryValidator()
    {
        RuleFor(query => query.CoachId).NotEmpty();
    }
}

