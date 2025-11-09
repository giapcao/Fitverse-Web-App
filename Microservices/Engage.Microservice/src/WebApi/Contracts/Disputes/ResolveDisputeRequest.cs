using Domain.Persistence.Enums;

namespace WebApi.Contracts.Disputes;

public record ResolveDisputeRequest(
    Guid ResolvedBy,
    DisputeStatus Status,
    string? ResolutionNotes);

