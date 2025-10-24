using System;
using System.Threading.Tasks;
using Application.Roles.Command;
using Domain.Enums;
using MassTransit;
using MediatR;
using SharedLibrary.Contracts.CoachProfileCreating;

namespace Application.Consumers;

public class CoachRoleAssignRequestedConsumer : IConsumer<CoachRoleAssignRequestedEvent>
{
    private readonly ISender _sender;

    public CoachRoleAssignRequestedConsumer(ISender sender)
    {
        _sender = sender;
    }

    public async Task Consume(ConsumeContext<CoachRoleAssignRequestedEvent> context)
    {
        try
        {
            var role = Enum.TryParse<RoleType>(context.Message.Role, true, out var roleType)
                ? roleType
                : RoleType.Coach;

            var result = await _sender.Send(
                new AssignRoleToUserCommand(context.Message.CoachId, role),
                context.CancellationToken);

            if (result.IsSuccess)
            {
                await context.Publish(new CoachRoleAssignedEvent
                {
                    CorrelationId = context.Message.CorrelationId,
                    CoachId = context.Message.CoachId,
                    Role = context.Message.Role
                });
                return;
            }

            await context.Publish(new CoachRoleAssignFailedEvent
            {
                CorrelationId = context.Message.CorrelationId,
                CoachId = context.Message.CoachId,
                Role = context.Message.Role,
                Reason = result.Error.Description
            });
        }
        catch (Exception ex)
        {
            await context.Publish(new CoachRoleAssignFailedEvent
            {
                CorrelationId = context.Message.CorrelationId,
                CoachId = context.Message.CoachId,
                Role = context.Message.Role,
                Reason = ex.Message
            });
        }
    }
}
