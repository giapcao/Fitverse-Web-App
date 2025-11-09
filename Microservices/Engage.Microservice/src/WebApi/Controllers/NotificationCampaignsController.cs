using Application.NotificationCampaigns.Command;
using Application.NotificationCampaigns.Query;
using Domain.Persistence.Enums;
using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using SharedLibrary.Common;
using WebApi.Contracts.Campaigns;

namespace WebApi.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/[controller]/v{version:apiVersion}")]
public class NotificationCampaignsController : ApiController
{
    public NotificationCampaignsController(IMediator mediator)
        : base(mediator)
    {
    }

    [HttpPost]
    [ProducesResponseType(typeof(object), StatusCodes.Status201Created)]
    public async Task<IActionResult> Create([FromBody] CreateNotificationCampaignRequest request, CancellationToken cancellationToken)
    {
        var command = new CreateNotificationCampaignCommand(
            request.Audience,
            request.TemplateKey,
            request.Title,
            request.Body,
            request.Data,
            request.ScheduledAt,
            request.CreatedBy);

        var result = await _mediator.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            return HandleFailure(result);
        }

        return CreatedAtAction(nameof(GetById), new { campaignId = result.Value.Id, version = "1.0" }, result.Value);
    }

    [HttpPost("{campaignId:guid}/schedule")]
    public async Task<IActionResult> Schedule(Guid campaignId, [FromBody] ScheduleNotificationCampaignRequest request, CancellationToken cancellationToken)
    {
        var command = new ScheduleNotificationCampaignCommand(campaignId, request.ScheduledAt);
        var result = await _mediator.Send(command, cancellationToken);

        return result.IsSuccess ? Ok(result.Value) : HandleFailure(result);
    }

    [HttpGet("{campaignId:guid}")]
    public async Task<IActionResult> GetById(Guid campaignId, CancellationToken cancellationToken)
    {
        var query = new GetNotificationCampaignByIdQuery(campaignId);
        var result = await _mediator.Send(query, cancellationToken);

        return result.IsSuccess ? Ok(result.Value) : HandleFailure(result);
    }

    [HttpGet]
    public async Task<IActionResult> List([FromQuery] CampaignStatus? status, CancellationToken cancellationToken)
    {
        var query = new ListNotificationCampaignsQuery(status);
        var result = await _mediator.Send(query, cancellationToken);

        return result.IsSuccess ? Ok(result.Value) : HandleFailure(result);
    }
}
