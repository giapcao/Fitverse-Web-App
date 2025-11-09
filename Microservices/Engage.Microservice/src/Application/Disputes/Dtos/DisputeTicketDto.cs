using Domain.Persistence.Enums;

namespace Application.Disputes.Dtos;

public record DisputeTicketDto(
    Guid Id,
    Guid BookingId,
    Guid OpenedBy,
    DisputeStatus Status,
    string? ReasonType,
    string? Description,
    IReadOnlyCollection<string>? EvidenceUrls,
    Guid? ResolvedBy,
    DateTime? ResolvedAt,
    DateTime CreatedAt,
    DateTime UpdatedAt);

