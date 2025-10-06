using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Application.CoachMedia.Command;
using Application.CoachMedia.Query;
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
public class CoachMediaController : ApiController
{
    public CoachMediaController(IMediator mediator) : base(mediator)
    {
    }

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<CoachMediaDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMedia([FromQuery] Guid? coachId, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new ListCoachMediaQuery(coachId), cancellationToken);
        if (result.IsFailure)
        {
            return HandleFailure(result);
        }

        return Ok(result.Value);
    }

    [HttpGet("{mediaId:guid}")]
    [ProducesResponseType(typeof(CoachMediaDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetMediaById(Guid mediaId, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetCoachMediaByIdQuery(mediaId), cancellationToken);
        if (result.IsFailure)
        {
            return HandleFailure(result);
        }

        return Ok(result.Value);
    }

    [HttpPost]
    [ProducesResponseType(typeof(CoachMediaDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateMedia([FromBody] CreateCoachMediaCommand command, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command, cancellationToken);
        if (result.IsFailure)
        {
            return HandleFailure(result);
        }

        var version = HttpContext.GetRequestedApiVersion()?.ToString() ?? "1.0";
        return CreatedAtAction(nameof(GetMediaById), new { mediaId = result.Value.Id, version }, result.Value);
    }

    [HttpPut("{mediaId:guid}")]
    [ProducesResponseType(typeof(CoachMediaDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateMedia(Guid mediaId, [FromBody] UpdateCoachMediaCommand command, CancellationToken cancellationToken)
    {
        var merged = command with { MediaId = mediaId };
        var result = await _mediator.Send(merged, cancellationToken);
        if (result.IsFailure)
        {
            return HandleFailure(result);
        }

        return Ok(result.Value);
    }

    [HttpDelete("{mediaId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteMedia(Guid mediaId, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new DeleteCoachMediaCommand(mediaId), cancellationToken);
        if (result.IsFailure)
        {
            return HandleFailure(result);
        }

        return NoContent();
    }
}
