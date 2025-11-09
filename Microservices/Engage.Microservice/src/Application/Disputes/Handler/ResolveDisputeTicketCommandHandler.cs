using Application.Abstractions.Messaging;
using Application.Disputes.Command;
using Application.Disputes.Dtos;
using Domain.IRepositories;
using Domain.Persistence.Enums;
using MapsterMapper;
using SharedLibrary.Common.ResponseModel;

namespace Application.Disputes.Handler;

public sealed class ResolveDisputeTicketCommandHandler
    : ICommandHandler<ResolveDisputeTicketCommand, DisputeTicketDto>
{
    private readonly IDisputeTicketRepository _disputeTicketRepository;
    private readonly IMapper _mapper;

    public ResolveDisputeTicketCommandHandler(IDisputeTicketRepository disputeTicketRepository, IMapper mapper)
    {
        _disputeTicketRepository = disputeTicketRepository;
        _mapper = mapper;
    }

    public async Task<Result<DisputeTicketDto>> Handle(ResolveDisputeTicketCommand request, CancellationToken cancellationToken)
    {
        var ticket = await _disputeTicketRepository.GetByIdAsync(request.DisputeId, cancellationToken);
        if (ticket is null)
        {
            return Result.Failure<DisputeTicketDto>(new Error("Dispute.NotFound", "Dispute ticket not found."));
        }

        if (ticket.Status is DisputeStatus.Resolved or DisputeStatus.Dismissed)
        {
            return Result.Failure<DisputeTicketDto>(new Error("Dispute.Completed", "Dispute ticket already resolved."));
        }

        ticket.Status = request.Status;
        ticket.ResolvedBy = request.ResolvedBy;
        ticket.ResolvedAt = DateTime.UtcNow;
        ticket.UpdatedAt = DateTime.UtcNow;

        if (!string.IsNullOrWhiteSpace(request.ResolutionNotes))
        {
            ticket.Description = string.IsNullOrWhiteSpace(ticket.Description)
                ? request.ResolutionNotes
                : $"{ticket.Description}{Environment.NewLine}{Environment.NewLine}Resolution:{Environment.NewLine}{request.ResolutionNotes}";
        }

        _disputeTicketRepository.Update(ticket);

        return Result.Success(_mapper.Map<DisputeTicketDto>(ticket));
    }
}

