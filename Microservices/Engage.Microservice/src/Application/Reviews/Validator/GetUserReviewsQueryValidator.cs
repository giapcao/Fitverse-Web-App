using Application.Reviews.Query;
using FluentValidation;

namespace Application.Reviews.Validator;

public sealed class GetUserReviewsQueryValidator : AbstractValidator<GetUserReviewsQuery>
{
    public GetUserReviewsQueryValidator()
    {
        RuleFor(query => query.UserId).NotEmpty();
    }
}

