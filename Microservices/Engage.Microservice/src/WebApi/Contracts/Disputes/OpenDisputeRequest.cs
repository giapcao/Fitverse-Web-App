namespace WebApi.Contracts.Disputes;

public record OpenDisputeRequest(
    Guid BookingId,
    Guid OpenedBy,
    string? ReasonType,
    string? Description,
    IReadOnlyCollection<string>? EvidenceUrls);

