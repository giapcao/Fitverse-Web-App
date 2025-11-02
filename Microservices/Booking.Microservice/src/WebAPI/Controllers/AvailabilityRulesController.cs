using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Application.AvailabilityRules.Commands;
using Application.AvailabilityRules.Queries;
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
public class AvailabilityRulesController : ApiController
{
    public AvailabilityRulesController(IMediator mediator) : base(mediator)
    {
    }

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyCollection<AvailabilityRuleDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAvailabilityRules([FromQuery] Guid? coachId, CancellationToken cancellationToken)
    {
        Result<IReadOnlyCollection<AvailabilityRuleDto>> result = coachId.HasValue
            ? await _mediator.Send(new ListAvailabilityRulesByCoachQuery(coachId.Value), cancellationToken)
            : await _mediator.Send(new ListAvailabilityRulesQuery(), cancellationToken);

        if (result.IsFailure)
        {
            return HandleFailure(result);
        }

        return Ok(result.Value);
    }

    [HttpGet("coach/{coachId:guid}")]
    [ProducesResponseType(typeof(IReadOnlyCollection<AvailabilityRuleDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAvailabilityRulesByCoach(Guid coachId, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new ListAvailabilityRulesByCoachQuery(coachId), cancellationToken);
        if (result.IsFailure)
        {
            return HandleFailure(result);
        }

        return Ok(result.Value);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(AvailabilityRuleDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetAvailabilityRuleById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetAvailabilityRuleByIdQuery(id), cancellationToken);
        if (result.IsFailure)
        {
            return HandleFailure(result);
        }

        return Ok(result.Value);
    }

    [HttpPost]
    [ProducesResponseType(typeof(AvailabilityRuleDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateAvailabilityRule([FromBody] CreateAvailabilityRuleCommand command, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command, cancellationToken);
        if (result.IsFailure)
        {
            return HandleFailure(result);
        }

        var version = HttpContext.GetRequestedApiVersion()?.ToString() ?? "1.0";
        return CreatedAtAction(nameof(GetAvailabilityRuleById), new { id = result.Value.Id, version }, result.Value);
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(AvailabilityRuleDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateAvailabilityRule(Guid id, [FromBody] UpdateAvailabilityRuleCommand command, CancellationToken cancellationToken)
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
    public async Task<IActionResult> DeleteAvailabilityRule(Guid id, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new DeleteAvailabilityRuleCommand(id), cancellationToken);
        if (result.IsFailure)
        {
            return HandleFailure(result);
        }

        return NoContent();
    }
}
