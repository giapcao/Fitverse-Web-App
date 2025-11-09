using Application.Conversations.Query;
using FluentValidation;

namespace Application.Conversations.Validator;

public sealed class GetConversationByParticipantsQueryValidator : AbstractValidator<GetConversationByParticipantsQuery>
{
    public GetConversationByParticipantsQueryValidator()
    {
        RuleFor(query => query.UserId).NotEmpty();
        RuleFor(query => query.CoachId).NotEmpty();
    }
}

