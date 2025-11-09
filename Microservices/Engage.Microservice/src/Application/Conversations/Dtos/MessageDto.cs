namespace Application.Conversations.Dtos;

public record MessageDto(
    Guid Id,
    Guid SenderId,
    string? Body,
    string? AttachmentUrl,
    DateTime CreatedAt);

