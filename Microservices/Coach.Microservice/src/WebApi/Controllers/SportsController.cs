using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Application.Features;
using Application.Sports.Command;
using Application.Sports.Query;
using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SharedLibrary.Common;

namespace WebAPI.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/[controller]/v{version:apiVersion}")]
public class SportsController : ApiController
{
    public SportsController(IMediator mediator) : base(mediator)
    {
    }

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<SportDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSports(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new ListSportsQuery(), cancellationToken);
        if (result.IsFailure)
        {
            return HandleFailure(result);
        }

        return Ok(result.Value);
    }

    [HttpGet("{sportId:guid}")]
    [ProducesResponseType(typeof(SportDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetSportById(Guid sportId, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetSportByIdQuery(sportId), cancellationToken);
        if (result.IsFailure)
        {
            return HandleFailure(result);
        }

        return Ok(result.Value);
    }

    [HttpPost]
    [ProducesResponseType(typeof(SportDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateSport([FromBody] CreateSportCommand command, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command, cancellationToken);
        if (result.IsFailure)
        {
            return HandleFailure(result);
        }

        var version = HttpContext.GetRequestedApiVersion()?.ToString() ?? "1.0";
        return CreatedAtAction(nameof(GetSportById), new { sportId = result.Value.Id, version }, result.Value);
    }

    [HttpPut("{sportId:guid}")]
    [ProducesResponseType(typeof(SportDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateSport(Guid sportId, [FromBody] UpdateSportCommand command, CancellationToken cancellationToken)
    {
        var merged = command with { SportId = sportId };
        var result = await _mediator.Send(merged, cancellationToken);
        if (result.IsFailure)
        {
            return HandleFailure(result);
        }

        return Ok(result.Value);
    }

    [HttpDelete("{sportId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteSport(Guid sportId, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new DeleteSportCommand(sportId), cancellationToken);
        if (result.IsFailure)
        {
            return HandleFailure(result);
        }

        return NoContent();
    }
}
