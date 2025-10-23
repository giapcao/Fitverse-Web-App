using System;

namespace SharedLibrary.Contracts.CoachProfileCreating;

public class CoachRoleAssignedEvent
{
    public Guid CorrelationId { get; set; }

    public Guid CoachId { get; set; }

    public string Role { get; set; } = default!;
}
