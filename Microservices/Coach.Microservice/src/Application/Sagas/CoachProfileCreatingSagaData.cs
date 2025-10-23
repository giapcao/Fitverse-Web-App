using System;
using MassTransit;

namespace Application.Sagas;

public class CoachProfileCreatingSagaData : SagaStateMachineInstance, ISagaVersion
{
    public Guid CorrelationId { get; set; }

    public string CurrentState { get; set; } = default!;

    public Guid CoachId { get; set; }

    public string Role { get; set; } = default!;

    public bool CoachProfileCreated { get; set; }

    public bool CoachRoleAssigned { get; set; }

    public string? FailureReason { get; set; }

    public int RetryCount { get; set; }

    public int Version { get; set; }
}
