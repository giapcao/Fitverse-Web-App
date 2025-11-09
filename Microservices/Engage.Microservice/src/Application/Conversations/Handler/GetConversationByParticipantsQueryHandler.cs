using Application.Abstractions.Messaging;
using Application.Conversations.Dtos;
using Application.Conversations.Query;
using Domain.IRepositories;
using MapsterMapper;
using SharedLibrary.Common.ResponseModel;

namespace Application.Conversations.Handler;

public sealed class GetConversationByParticipantsQueryHandler
    : IQueryHandler<GetConversationByParticipantsQuery, ConversationDto>
{
    private readonly IConversationRepository _conversationRepository;
    private readonly IMapper _mapper;

    public GetConversationByParticipantsQueryHandler(IConversationRepository conversationRepository, IMapper mapper)
    {
        _conversationRepository = conversationRepository;
        _mapper = mapper;
    }

    public async Task<Result<ConversationDto>> Handle(GetConversationByParticipantsQuery request, CancellationToken cancellationToken)
    {
        var conversation = await _conversationRepository.GetByParticipantsAsync(request.UserId, request.CoachId, cancellationToken, asNoTracking: true);
        if (conversation is null)
        {
            return Result.Failure<ConversationDto>(new Error("Conversation.NotFound", "Conversation does not exist."));
        }

        return Result.Success(_mapper.Map<ConversationDto>(conversation));
    }
}

