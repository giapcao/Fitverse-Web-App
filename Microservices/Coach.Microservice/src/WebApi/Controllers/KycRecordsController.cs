using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Application.Features;
using Application.KycRecords.Command;
using Application.KycRecords.Query;
using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SharedLibrary.Common;

namespace WebAPI.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/[controller]/v{version:apiVersion}")]
public class KycRecordsController : ApiController
{
    public KycRecordsController(IMediator mediator) : base(mediator)
    {
    }

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<KycRecordDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetRecords([FromQuery] Guid? coachId, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new ListKycRecordsQuery(coachId), cancellationToken);
        if (result.IsFailure)
        {
            return HandleFailure(result);
        }

        return Ok(result.Value);
    }

    [HttpGet("{recordId:guid}")]
    [ProducesResponseType(typeof(KycRecordDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetRecordById(Guid recordId, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetKycRecordByIdQuery(recordId), cancellationToken);
        if (result.IsFailure)
        {
            return HandleFailure(result);
        }

        return Ok(result.Value);
    }

    [HttpGet("coach/{coachId:guid}/latest")]
    [ProducesResponseType(typeof(KycRecordDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetLatestRecord(Guid coachId, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetLatestKycRecordByCoachQuery(coachId), cancellationToken);
        if (result.IsFailure)
        {
            return HandleFailure(result);
        }

        return Ok(result.Value);
    }

    [HttpPost]
    [ProducesResponseType(typeof(KycRecordDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateRecord([FromBody] CreateKycRecordCommand command, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command, cancellationToken);
        if (result.IsFailure)
        {
            return HandleFailure(result);
        }

        var version = HttpContext.GetRequestedApiVersion()?.ToString() ?? "1.0";
        return CreatedAtAction(nameof(GetRecordById), new { recordId = result.Value.Id, version }, result.Value);
    }

    [HttpPut("{recordId:guid}/status")]
    [ProducesResponseType(typeof(KycRecordDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateStatus(Guid recordId, [FromBody] UpdateKycRecordStatusCommand command, CancellationToken cancellationToken)
    {
        var merged = command with { RecordId = recordId };
        var result = await _mediator.Send(merged, cancellationToken);
        if (result.IsFailure)
        {
            return HandleFailure(result);
        }

        return Ok(result.Value);
    }

    [HttpDelete("{recordId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteRecord(Guid recordId, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new DeleteKycRecordCommand(recordId), cancellationToken);
        if (result.IsFailure)
        {
            return HandleFailure(result);
        }

        return NoContent();
    }
}
