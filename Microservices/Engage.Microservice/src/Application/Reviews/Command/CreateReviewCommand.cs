using Application.Abstractions.Messaging;
using Application.Reviews.Dtos;

namespace Application.Reviews.Command;

public sealed record CreateReviewCommand(
    Guid BookingId,
    Guid UserId,
    Guid CoachId,
    int Rating,
    string? Comment,
    bool IsPublic) : ICommand<ReviewDto>;

