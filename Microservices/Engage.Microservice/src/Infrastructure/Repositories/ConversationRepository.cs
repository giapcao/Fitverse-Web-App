using Domain.IRepositories;
using Domain.Persistence.Models;
using Infrastructure.Common;
using Infrastructure.Context;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class ConversationRepository : Repository<Conversation>, IConversationRepository
{
    private readonly FitverseEngageDbContext _context;

    public ConversationRepository(FitverseEngageDbContext context)
        : base(context)
    {
        _context = context;
    }

    public async Task<Conversation?> GetByParticipantsAsync(Guid userId, Guid coachId, CancellationToken cancellationToken, bool asNoTracking = false)
    {
        var query = _context.Conversations
            .Include(conversation => conversation.Messages)
            .Where(conversation => conversation.UserId == userId && conversation.CoachId == coachId);

        if (asNoTracking)
        {
            query = query.AsNoTracking();
        }

        return await query.FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<Conversation?> GetThreadByIdAsync(Guid conversationId, CancellationToken cancellationToken, bool asNoTracking = false)
    {
        var query = _context.Conversations
            .Include(conversation => conversation.Messages)
            .Where(conversation => conversation.Id == conversationId);

        if (asNoTracking)
        {
            query = query.AsNoTracking();
        }

        return await query.FirstOrDefaultAsync(cancellationToken);
    }

    public async Task AddMessageAsync(Message message, CancellationToken cancellationToken)
    {
        await _context.Messages.AddAsync(message, cancellationToken);
    }
}

