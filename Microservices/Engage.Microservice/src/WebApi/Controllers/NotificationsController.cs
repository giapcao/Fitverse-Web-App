using Application.Notifications.Command;
using Application.Notifications.Query;
using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using SharedLibrary.Common;
using WebApi.Contracts.Notifications;

namespace WebApi.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/[controller]/v{version:apiVersion}")]
public class NotificationsController : ApiController
{
    public NotificationsController(IMediator mediator)
        : base(mediator)
    {
    }

    [HttpPost]
    public async Task<IActionResult> Send([FromBody] SendNotificationRequest request, CancellationToken cancellationToken)
    {
        var command = new SendNotificationCommand(
            request.UserId,
            request.Channel,
            request.Title,
            request.Body,
            request.Data,
            request.CampaignId);

        var result = await _mediator.Send(command, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : HandleFailure(result);
    }

    [HttpGet]
    public async Task<IActionResult> List([FromQuery] Guid userId, [FromQuery] int? take, CancellationToken cancellationToken)
    {
        var query = new ListNotificationsByUserQuery(userId, take);
        var result = await _mediator.Send(query, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : HandleFailure(result);
    }
}
