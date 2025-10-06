using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Application.CoachServices.Command;
using Application.CoachServices.Query;
using Application.Features;
using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SharedLibrary.Common;

namespace WebAPI.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/[controller]/v{version:apiVersion}")]
public class CoachServicesController : ApiController
{
    public CoachServicesController(IMediator mediator) : base(mediator)
    {
    }

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<CoachServiceDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetServices([FromQuery] Guid? coachId, [FromQuery] Guid? sportId, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new ListCoachServicesQuery(coachId, sportId), cancellationToken);
        if (result.IsFailure)
        {
            return HandleFailure(result);
        }

        return Ok(result.Value);
    }

    [HttpGet("{serviceId:guid}")]
    [ProducesResponseType(typeof(CoachServiceDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetServiceById(Guid serviceId, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetCoachServiceByIdQuery(serviceId), cancellationToken);
        if (result.IsFailure)
        {
            return HandleFailure(result);
        }

        return Ok(result.Value);
    }

    [HttpPost]
    [ProducesResponseType(typeof(CoachServiceDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateService([FromBody] CreateCoachServiceCommand command, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command, cancellationToken);
        if (result.IsFailure)
        {
            return HandleFailure(result);
        }

        var version = HttpContext.GetRequestedApiVersion()?.ToString() ?? "1.0";
        return CreatedAtAction(nameof(GetServiceById), new { serviceId = result.Value.Id, version }, result.Value);
    }

    [HttpPut("{serviceId:guid}")]
    [ProducesResponseType(typeof(CoachServiceDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateService(Guid serviceId, [FromBody] UpdateCoachServiceCommand command, CancellationToken cancellationToken)
    {
        var merged = command with { ServiceId = serviceId };
        var result = await _mediator.Send(merged, cancellationToken);
        if (result.IsFailure)
        {
            return HandleFailure(result);
        }

        return Ok(result.Value);
    }

    [HttpDelete("{serviceId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteService(Guid serviceId, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new DeleteCoachServiceCommand(serviceId), cancellationToken);
        if (result.IsFailure)
        {
            return HandleFailure(result);
        }

        return NoContent();
    }
}
