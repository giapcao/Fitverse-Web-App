using Application.Abstractions.Messaging;
using Application.Disputes.Command;
using Application.Disputes.Dtos;
using Domain.IRepositories;
using Domain.Persistence.Enums;
using Domain.Persistence.Models;
using MapsterMapper;
using SharedLibrary.Common.ResponseModel;

namespace Application.Disputes.Handler;

public sealed class OpenDisputeTicketCommandHandler
    : ICommandHandler<OpenDisputeTicketCommand, DisputeTicketDto>
{
    private readonly IDisputeTicketRepository _disputeTicketRepository;
    private readonly IMapper _mapper;

    public OpenDisputeTicketCommandHandler(IDisputeTicketRepository disputeTicketRepository, IMapper mapper)
    {
        _disputeTicketRepository = disputeTicketRepository;
        _mapper = mapper;
    }

    public async Task<Result<DisputeTicketDto>> Handle(OpenDisputeTicketCommand request, CancellationToken cancellationToken)
    {
        var exists = await _disputeTicketRepository.ExistsForBookingAsync(request.BookingId, cancellationToken);
        if (exists)
        {
            return Result.Failure<DisputeTicketDto>(new Error("Dispute.Exists", "A dispute for this booking already exists."));
        }

        var now = DateTime.UtcNow;
        var ticket = new DisputeTicket
        {
            Id = Guid.NewGuid(),
            BookingId = request.BookingId,
            OpenedBy = request.OpenedBy,
            ReasonType = request.ReasonType,
            Description = request.Description,
            EvidenceUrls = request.EvidenceUrls?.ToList(),
            Status = DisputeStatus.Open,
            CreatedAt = now,
            UpdatedAt = now
        };

        await _disputeTicketRepository.AddAsync(ticket, cancellationToken);

        return Result.Success(_mapper.Map<DisputeTicketDto>(ticket));
    }
}

