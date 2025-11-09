using Application.Abstractions.Messaging;
using Application.Conversations.Dtos;

namespace Application.Conversations.Query;

public sealed record GetConversationByParticipantsQuery(Guid UserId, Guid CoachId) : IQuery<ConversationDto>;

