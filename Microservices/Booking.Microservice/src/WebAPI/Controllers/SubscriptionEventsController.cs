using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Application.Features;
using Application.SubscriptionEvents.Commands;
using Application.SubscriptionEvents.Queries;
using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SharedLibrary.Common;
using SharedLibrary.Common.ResponseModel;

namespace WebAPI.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/[controller]/v{version:apiVersion}")]
public sealed class SubscriptionEventsController : ApiController
{
    public SubscriptionEventsController(IMediator mediator) : base(mediator)
    {
    }

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyCollection<SubscriptionEventDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSubscriptionEvents(
        [FromQuery] Guid? subscriptionId,
        [FromQuery] Guid? bookingId,
        CancellationToken cancellationToken)
    {
        Result<IReadOnlyCollection<SubscriptionEventDto>> result;
        if (subscriptionId.HasValue)
        {
            result = await _mediator.Send(new ListSubscriptionEventsBySubscriptionQuery(subscriptionId.Value), cancellationToken);
        }
        else if (bookingId.HasValue)
        {
            result = await _mediator.Send(new ListSubscriptionEventsByBookingQuery(bookingId.Value), cancellationToken);
        }
        else
        {
            result = await _mediator.Send(new ListSubscriptionEventsQuery(), cancellationToken);
        }

        if (result.IsFailure)
        {
            return HandleFailure(result);
        }

        return Ok(result.Value);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(SubscriptionEventDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetSubscriptionEventById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetSubscriptionEventByIdQuery(id), cancellationToken);
        if (result.IsFailure)
        {
            return HandleFailure(result);
        }

        return Ok(result.Value);
    }

    [HttpPost]
    [ProducesResponseType(typeof(SubscriptionEventDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateSubscriptionEvent([FromBody] CreateSubscriptionEventCommand command, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command, cancellationToken);
        if (result.IsFailure)
        {
            return HandleFailure(result);
        }

        var version = HttpContext.GetRequestedApiVersion()?.ToString() ?? "1.0";
        return CreatedAtAction(nameof(GetSubscriptionEventById), new { id = result.Value.Id, version }, result.Value);
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(SubscriptionEventDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateSubscriptionEvent(Guid id, [FromBody] UpdateSubscriptionEventCommand command, CancellationToken cancellationToken)
    {
        if (command.Id != id)
        {
            return BadRequest("Route id and payload id must match.");
        }

        var result = await _mediator.Send(command, cancellationToken);
        if (result.IsFailure)
        {
            return HandleFailure(result);
        }

        return Ok(result.Value);
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteSubscriptionEvent(Guid id, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new DeleteSubscriptionEventCommand(id), cancellationToken);
        if (result.IsFailure)
        {
            return HandleFailure(result);
        }

        return NoContent();
    }
}
