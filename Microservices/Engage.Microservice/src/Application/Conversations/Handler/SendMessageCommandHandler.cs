using Application.Abstractions.Messaging;
using Application.Conversations.Command;
using Application.Conversations.Dtos;
using Domain.IRepositories;
using Domain.Persistence.Models;
using MapsterMapper;
using SharedLibrary.Common.ResponseModel;

namespace Application.Conversations.Handler;

public sealed class SendMessageCommandHandler
    : ICommandHandler<SendMessageCommand, ConversationDto>
{
    private readonly IConversationRepository _conversationRepository;
    private readonly IMapper _mapper;

    public SendMessageCommandHandler(IConversationRepository conversationRepository, IMapper mapper)
    {
        _conversationRepository = conversationRepository;
        _mapper = mapper;
    }

    public async Task<Result<ConversationDto>> Handle(SendMessageCommand request, CancellationToken cancellationToken)
    {
        var conversation = await _conversationRepository.GetByParticipantsAsync(request.UserId, request.CoachId, cancellationToken);

        if (conversation is null)
        {
            conversation = new Conversation
            {
                Id = Guid.NewGuid(),
                UserId = request.UserId,
                CoachId = request.CoachId,
                CreatedAt = DateTime.UtcNow
            };

            await _conversationRepository.AddAsync(conversation, cancellationToken);
        }

        var message = new Message
        {
            Id = Guid.NewGuid(),
            ConversationId = conversation.Id,
            SenderId = request.SenderId,
            Body = request.Body,
            AttachmentUrl = request.AttachmentUrl,
            CreatedAt = DateTime.UtcNow
        };

        await _conversationRepository.AddMessageAsync(message, cancellationToken);

        conversation.Messages.Add(message);

        var dto = _mapper.Map<ConversationDto>(conversation);
        return Result.Success(dto);
    }
}

