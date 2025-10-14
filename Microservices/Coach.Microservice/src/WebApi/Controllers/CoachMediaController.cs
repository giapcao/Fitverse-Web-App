using System;
using System.IO;
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
using SharedLibrary.Common.ResponseModel;
using WebAPI.Contracts.Requests;

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
    [ProducesResponseType(typeof(PagedResult<CoachMediaDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMedia(
        [FromQuery] Guid? coachId,
        [FromQuery] bool? isFeatured = null,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {

        var result = await _mediator.Send(new ListCoachMediaQuery(coachId, isFeatured, pageNumber, pageSize), cancellationToken);
        if (result.IsFailure)
        {
            return HandleFailure(result);
        }

        return Ok(result.Value);
    }

    [HttpGet("all")]
    [ProducesResponseType(typeof(PagedResult<CoachMediaDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllMedia(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(new ListAllCoachMediaQuery(pageNumber, pageSize), cancellationToken);
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
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> CreateMedia([FromForm] CreateCoachMediaRequest request, CancellationToken cancellationToken)
    {
        CoachMediaFile? file = null;
        if (request.File is not null && request.File.Length > 0)
        {
            using var memoryStream = new MemoryStream();
            await request.File.CopyToAsync(memoryStream, cancellationToken);
            var directory = string.IsNullOrWhiteSpace(request.Directory) ? "media" : request.Directory;
            file = new CoachMediaFile(
                memoryStream.ToArray(),
                request.File.FileName,
                request.File.ContentType,
                directory);
        }

        var command = new CreateCoachMediaCommand(
            request.CoachId,
            request.MediaName,
            request.Description,
            request.MediaType,
            request.Url,
            request.IsFeatured,
            request.Directory,
            file);

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
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> UpdateMedia(Guid mediaId, [FromForm] UpdateCoachMediaRequest request, CancellationToken cancellationToken)
    {
        CoachMediaFile? file = null;
        if (request.File is not null && request.File.Length > 0)
        {
            using var memoryStream = new MemoryStream();
            await request.File.CopyToAsync(memoryStream, cancellationToken);
            var directory = string.IsNullOrWhiteSpace(request.Directory) ? "media" : request.Directory;
            file = new CoachMediaFile(
                memoryStream.ToArray(),
                request.File.FileName,
                request.File.ContentType,
                directory);
        }

        var command = new UpdateCoachMediaCommand(
            mediaId,
            request.MediaName,
            request.Description,
            request.MediaType,
            request.Url,
            request.IsFeatured,
            request.Directory,
            file);

        var result = await _mediator.Send(command, cancellationToken);
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
