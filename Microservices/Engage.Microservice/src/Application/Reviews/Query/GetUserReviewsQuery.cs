using Application.Abstractions.Messaging;
using Application.Reviews.Dtos;

namespace Application.Reviews.Query;

public sealed record GetUserReviewsQuery(Guid UserId) : IQuery<IReadOnlyList<ReviewDto>>;

