using Application.Abstractions.Messaging;
using Application.Conversations.Dtos;

namespace Application.Conversations.Command;

public sealed record SendMessageCommand(
    Guid UserId,
    Guid CoachId,
    Guid SenderId,
    string? Body,
    string? AttachmentUrl) : ICommand<ConversationDto>;

