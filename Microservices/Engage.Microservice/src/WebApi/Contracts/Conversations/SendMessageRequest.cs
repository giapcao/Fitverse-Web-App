namespace WebApi.Contracts.Conversations;

public record SendMessageRequest(
    Guid UserId,
    Guid CoachId,
    Guid SenderId,
    string? Body,
    string? AttachmentUrl);

