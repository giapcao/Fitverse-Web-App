using Application.Abstractions.Messaging;
using Application.Disputes.Dtos;
using Domain.Persistence.Enums;

namespace Application.Disputes.Command;

public sealed record ResolveDisputeTicketCommand(
    Guid DisputeId,
    Guid ResolvedBy,
    DisputeStatus Status,
    string? ResolutionNotes) : ICommand<DisputeTicketDto>;

