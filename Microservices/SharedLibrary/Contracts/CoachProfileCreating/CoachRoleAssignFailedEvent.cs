using System;

namespace SharedLibrary.Contracts.CoachProfileCreating;

public class CoachRoleAssignFailedEvent
{
    public Guid CorrelationId { get; set; }

    public Guid CoachId { get; set; }

    public string Role { get; set; } = default!;

    public string Reason { get; set; } = default!;
}
