using Application.Notifications.Query;
using FluentValidation;

namespace Application.Notifications.Validator;

public sealed class ListNotificationsByUserQueryValidator : AbstractValidator<ListNotificationsByUserQuery>
{
    public ListNotificationsByUserQueryValidator()
    {
        RuleFor(query => query.UserId).NotEmpty();
        RuleFor(query => query.Take)
            .GreaterThan(0)
            .LessThanOrEqualTo(200)
            .When(query => query.Take.HasValue);
    }
}

