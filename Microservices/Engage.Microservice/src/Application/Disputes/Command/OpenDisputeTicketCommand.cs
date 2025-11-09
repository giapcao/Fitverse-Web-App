using Application.Abstractions.Messaging;
using Application.Disputes.Dtos;

namespace Application.Disputes.Command;

public sealed record OpenDisputeTicketCommand(
    Guid BookingId,
    Guid OpenedBy,
    string? ReasonType,
    string? Description,
    IReadOnlyCollection<string>? EvidenceUrls) : ICommand<DisputeTicketDto>;

