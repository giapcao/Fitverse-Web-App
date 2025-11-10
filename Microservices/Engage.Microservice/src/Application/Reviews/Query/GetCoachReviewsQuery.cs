using Application.Abstractions.Messaging;
using Application.Reviews.Dtos;

namespace Application.Reviews.Query;

public sealed record GetCoachReviewsQuery(Guid CoachId) : IQuery<IReadOnlyList<ReviewDto>>;

