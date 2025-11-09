using Domain.Persistence.Models;
using SharedLibrary.Common;

namespace Domain.IRepositories;

public interface IConversationRepository : IRepository<Conversation>
{
    Task<Conversation?> GetByParticipantsAsync(Guid userId, Guid coachId, CancellationToken cancellationToken, bool asNoTracking = false);

    Task<Conversation?> GetThreadByIdAsync(Guid conversationId, CancellationToken cancellationToken, bool asNoTracking = false);

    Task AddMessageAsync(Message message, CancellationToken cancellationToken);
}

