namespace Application.Reviews.Dtos;

public record ReviewDto(
    Guid Id,
    Guid BookingId,
    Guid UserId,
    Guid CoachId,
    int Rating,
    string? Comment,
    bool IsPublic,
    DateTime CreatedAt);

