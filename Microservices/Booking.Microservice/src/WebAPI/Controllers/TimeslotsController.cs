using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Application.Features;
using Application.Timeslots.Commands;
using Application.Timeslots.Queries;
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
public sealed class TimeslotsController : ApiController
{
    public TimeslotsController(IMediator mediator) : base(mediator)
    {
    }

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyCollection<TimeslotDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetTimeslots(
        [FromQuery] Guid? coachId,
        [FromQuery] bool openOnly = false,
        [FromQuery] DateTime? from = null,
        [FromQuery] DateTime? to = null,
        CancellationToken cancellationToken = default)
    {
        Result<IReadOnlyCollection<TimeslotDto>> result;

        if (openOnly)
        {
            if (!coachId.HasValue || !from.HasValue || !to.HasValue)
            {
                return BadRequest("Open timeslot requests require coachId, from, and to query parameters.");
            }

            result = await _mediator.Send(
                new ListOpenTimeslotsByCoachQuery(coachId.Value, from.Value, to.Value),
                cancellationToken);
        }
        else if (coachId.HasValue)
        {
            result = await _mediator.Send(new ListTimeslotsByCoachQuery(coachId.Value), cancellationToken);
        }
        else
        {
            result = await _mediator.Send(new ListTimeslotsQuery(), cancellationToken);
        }

        if (result.IsFailure)
        {
            return HandleFailure(result);
        }

        return Ok(result.Value);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(TimeslotDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetTimeslotById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetTimeslotByIdQuery(id), cancellationToken);
        if (result.IsFailure)
        {
            return HandleFailure(result);
        }

        return Ok(result.Value);
    }

    [HttpPost]
    [ProducesResponseType(typeof(TimeslotDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateTimeslot([FromBody] CreateTimeslotCommand command, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command, cancellationToken);
        if (result.IsFailure)
        {
            return HandleFailure(result);
        }

        var version = HttpContext.GetRequestedApiVersion()?.ToString() ?? "1.0";
        return CreatedAtAction(nameof(GetTimeslotById), new { id = result.Value.Id, version }, result.Value);
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(TimeslotDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateTimeslot(Guid id, [FromBody] UpdateTimeslotCommand command, CancellationToken cancellationToken)
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
    public async Task<IActionResult> DeleteTimeslot(Guid id, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new DeleteTimeslotCommand(id), cancellationToken);
        if (result.IsFailure)
        {
            return HandleFailure(result);
        }

        return NoContent();
    }
}
