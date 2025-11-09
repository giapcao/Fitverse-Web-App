namespace Application.Conversations.Dtos;

public record ConversationDto(
    Guid Id,
    Guid UserId,
    Guid CoachId,
    DateTime CreatedAt,
    IReadOnlyCollection<MessageDto> Messages);

