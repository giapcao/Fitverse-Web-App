using Application.Abstractions.Messaging;
using Application.Reviews.Dtos;

namespace Application.Reviews.Query;

public sealed record GetReviewByBookingIdQuery(Guid BookingId) : IQuery<ReviewDto>;

