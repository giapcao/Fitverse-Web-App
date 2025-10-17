using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Application.CoachTimeoffs.Commands;
using Application.CoachTimeoffs.Queries;
using Application.Features;
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
public class CoachTimeoffsController : ApiController
{
    public CoachTimeoffsController(IMediator mediator) : base(mediator)
    {
    }

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyCollection<CoachTimeoffDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCoachTimeoffs([FromQuery] Guid? coachId, CancellationToken cancellationToken)
    {
        Result<IReadOnlyCollection<CoachTimeoffDto>> result = coachId.HasValue
            ? await _mediator.Send(new ListCoachTimeoffsByCoachQuery(coachId.Value), cancellationToken)
            : await _mediator.Send(new ListCoachTimeoffsQuery(), cancellationToken);

        if (result.IsFailure)
        {
            return HandleFailure(result);
        }

        return Ok(result.Value);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(CoachTimeoffDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetCoachTimeoffById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetCoachTimeoffByIdQuery(id), cancellationToken);
        if (result.IsFailure)
        {
            return HandleFailure(result);
        }

        return Ok(result.Value);
    }

    [HttpPost]
    [ProducesResponseType(typeof(CoachTimeoffDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateCoachTimeoff([FromBody] CreateCoachTimeoffCommand command, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command, cancellationToken);
        if (result.IsFailure)
        {
            return HandleFailure(result);
        }

        var version = HttpContext.GetRequestedApiVersion()?.ToString() ?? "1.0";
        return CreatedAtAction(nameof(GetCoachTimeoffById), new { id = result.Value.Id, version }, result.Value);
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(CoachTimeoffDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateCoachTimeoff(Guid id, [FromBody] UpdateCoachTimeoffCommand command, CancellationToken cancellationToken)
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
    public async Task<IActionResult> DeleteCoachTimeoff(Guid id, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new DeleteCoachTimeoffCommand(id), cancellationToken);
        if (result.IsFailure)
        {
            return HandleFailure(result);
        }

        return NoContent();
    }
}
