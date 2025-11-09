using Application.Abstractions.Messaging;
using Application.Disputes.Dtos;
using Domain.Persistence.Enums;

namespace Application.Disputes.Query;

public sealed record ListDisputeTicketsQuery(DisputeStatus? Status) : IQuery<IReadOnlyList<DisputeTicketDto>>;

