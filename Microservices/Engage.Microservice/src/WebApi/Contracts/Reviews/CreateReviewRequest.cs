namespace WebApi.Contracts.Reviews;

public record CreateReviewRequest(
    Guid BookingId,
    Guid UserId,
    Guid CoachId,
    int Rating,
    string? Comment,
    bool IsPublic);

