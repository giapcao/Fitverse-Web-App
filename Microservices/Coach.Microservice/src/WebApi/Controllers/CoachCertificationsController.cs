using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Application.CoachCertifications.Command;
using Application.CoachCertifications.Query;
using Application.Features;
using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SharedLibrary.Common;

namespace WebAPI.Controllers;

[Authorize(Policy = "IsAdmin")]
[ApiController]
[ApiVersion("1.0")]
[Route("api/[controller]/v{version:apiVersion}")]
public class CoachCertificationsController : ApiController
{
    public CoachCertificationsController(IMediator mediator) : base(mediator)
    {
    }

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<CoachCertificationDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCertifications([FromQuery] Guid? coachId, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new ListCoachCertificationsQuery(coachId), cancellationToken);
        if (result.IsFailure)
        {
            return HandleFailure(result);
        }

        return Ok(result.Value);
    }

    [HttpGet("{certificationId:guid}")]
    [ProducesResponseType(typeof(CoachCertificationDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetCertificationById(Guid certificationId, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetCoachCertificationByIdQuery(certificationId), cancellationToken);
        if (result.IsFailure)
        {
            return HandleFailure(result);
        }

        return Ok(result.Value);
    }

    [HttpPost]
    [ProducesResponseType(typeof(CoachCertificationDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateCertification([FromBody] CreateCoachCertificationCommand command, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command, cancellationToken);
        if (result.IsFailure)
        {
            return HandleFailure(result);
        }

        var version = HttpContext.GetRequestedApiVersion()?.ToString() ?? "1.0";
        return CreatedAtAction(nameof(GetCertificationById), new { certificationId = result.Value.Id, version }, result.Value);
    }

    [HttpPut("{certificationId:guid}")]
    [ProducesResponseType(typeof(CoachCertificationDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateCertification(Guid certificationId, [FromBody] UpdateCoachCertificationCommand command, CancellationToken cancellationToken)
    {
        var merged = command with { CertificationId = certificationId };
        var result = await _mediator.Send(merged, cancellationToken);
        if (result.IsFailure)
        {
            return HandleFailure(result);
        }

        return Ok(result.Value);
    }

    [HttpDelete("{certificationId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteCertification(Guid certificationId, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new DeleteCoachCertificationCommand(certificationId), cancellationToken);
        if (result.IsFailure)
        {
            return HandleFailure(result);
        }

        return NoContent();
    }
}
