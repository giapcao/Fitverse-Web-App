using System;

namespace SharedLibrary.Contracts.CoachProfileCreating;

public class CoachRoleAssignRequestedEvent
{
    public Guid CorrelationId { get; set; }

    public Guid CoachId { get; set; }

    public string Role { get; set; } = default!;
}
