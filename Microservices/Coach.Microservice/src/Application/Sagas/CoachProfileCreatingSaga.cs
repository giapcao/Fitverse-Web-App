using System;
using System.Threading.Tasks;
using MassTransit;
using SharedLibrary.Contracts.CoachProfileCreating;

namespace Application.Sagas;

public class CoachProfileCreatingSaga : MassTransitStateMachine<CoachProfileCreatingSagaData>
{
    public State RoleAssigning { get; set; }
    public State Completed { get; set; }
    public State Failed { get; set; }

    public Event<CoachProfileCreatingSagaStart> CoachProfileCreated { get; set; }
    public Event<CoachRoleAssignedEvent> CoachRoleAssigned { get; set; }
    public Event<CoachRoleAssignFailedEvent> CoachRoleAssignFailed { get; set; }

    public CoachProfileCreatingSaga()
    {
        InstanceState(x => x.CurrentState);

        Event(() => CoachProfileCreated, e => e.CorrelateById(m => m.Message.CorrelationId));
        Event(() => CoachRoleAssigned, e => e.CorrelateById(m => m.Message.CorrelationId));
        Event(() => CoachRoleAssignFailed, e => e.CorrelateById(m => m.Message.CorrelationId));

        Initially(
            When(CoachProfileCreated)
                .Then(context =>
                {
                    context.Saga.CorrelationId = context.Message.CorrelationId;
                    context.Saga.CoachId = context.Message.CoachId;
                    context.Saga.Role = context.Message.Role;
                    context.Saga.CoachProfileCreated = true;
                })
                .TransitionTo(RoleAssigning)
                .ThenAsync(async context =>
                {
                    await context.Publish(new CoachRoleAssignRequestedEvent
                    {
                        CorrelationId = context.Message.CorrelationId,
                        CoachId = context.Message.CoachId,
                        Role = context.Message.Role
                    });
                })
        );

        During(RoleAssigning,
            When(CoachRoleAssigned)
                .Then(context =>
                {
                    context.Saga.CoachRoleAssigned = true;
                })
                .TransitionTo(Completed),
            When(CoachRoleAssignFailed)
                .Then(context =>
                {
                    context.Saga.FailureReason = context.Message.Reason;
                })
                .TransitionTo(Failed)
        );

        SetCompletedWhenFinalized();
    }
}
