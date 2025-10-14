using System;
using System.IO;
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
using System.Text.Json;
using SharedLibrary.Common;
using SharedLibrary.Common.ResponseModel;
using WebAPI.Contracts.Requests;

namespace WebAPI.Controllers;

// [Authorize(Policy = "IsAdmin")]
[ApiController]
[ApiVersion("1.0")]
[Route("api/[controller]/v{version:apiVersion}")]
public class CoachCertificationsController : ApiController
{

    public CoachCertificationsController(IMediator mediator) : base(mediator)
    {
    }

    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<CoachCertificationDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCertifications(
        [FromQuery] Guid? coachId,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(new ListCoachCertificationsQuery(coachId, pageNumber, pageSize), cancellationToken);
        if (result.IsFailure)
        {
            return HandleFailure(result);
        }

        return Ok(result.Value);
    }

    [HttpGet("all")]
    [ProducesResponseType(typeof(PagedResult<CoachCertificationDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllCertifications(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(new ListAllCoachCertificationsQuery(pageNumber, pageSize), cancellationToken);
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
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> CreateCertification([FromForm] CreateCoachCertificationRequest request, CancellationToken cancellationToken)
    {
        CoachCertificationFile? file = null;
        if (request.File is not null && request.File.Length > 0)
        {
            using var memoryStream = new MemoryStream();
            await request.File.CopyToAsync(memoryStream, cancellationToken);
            var directory = string.IsNullOrWhiteSpace(request.Directory) ? "certifications" : request.Directory;
            file = new CoachCertificationFile(
                memoryStream.ToArray(),
                request.File.FileName,
                request.File.ContentType,
                directory);
        }
        var command = new CreateCoachCertificationCommand(
            request.CoachId,
            request.CertName,
            request.Issuer,
            request.IssuedOn,
            request.ExpiresOn,
            request.FileUrl,
            request.Directory,
            file);
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
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> UpdateCertification(Guid certificationId, [FromForm] UpdateCoachCertificationRequest request, CancellationToken cancellationToken)
    {
        CoachCertificationFile? file = null;
        if (request.File is not null && request.File.Length > 0)
        {
            using var memoryStream = new MemoryStream();
            await request.File.CopyToAsync(memoryStream, cancellationToken);
            var directory = string.IsNullOrWhiteSpace(request.Directory) ? "certifications" : request.Directory;
            file = new CoachCertificationFile(
                memoryStream.ToArray(),
                request.File.FileName,
                request.File.ContentType,
                directory);
        }

        var command = new UpdateCoachCertificationCommand(
            certificationId,
            request.CertName,
            request.Issuer,
            request.IssuedOn,
            request.ExpiresOn,
            request.FileUrl,
            request.Directory,
            file);

        var result = await _mediator.Send(command, cancellationToken);
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

    [HttpPost("{certificationId:guid}/activate")]
    [ProducesResponseType(typeof(CoachCertificationDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ActivateCertification(Guid certificationId, CancellationToken cancellationToken, [FromQuery] Guid? reviewedBy = null)
    {
        var result = await _mediator.Send(new ActivateCoachCertificationCommand(certificationId, reviewedBy), cancellationToken);
        if (result.IsFailure)
        {
            return HandleFailure(result);
        }

        return Ok(result.Value);
    }

    [HttpPost("{certificationId:guid}/deactivate")]
    [ProducesResponseType(typeof(CoachCertificationDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeactivateCertification(Guid certificationId, CancellationToken cancellationToken, [FromQuery] Guid? reviewedBy = null)
    {
        var result = await _mediator.Send(new InactivateCoachCertificationCommand(certificationId, reviewedBy), cancellationToken);
        if (result.IsFailure)
        {
            return HandleFailure(result);
        }

        return Ok(result.Value);
    }
}
