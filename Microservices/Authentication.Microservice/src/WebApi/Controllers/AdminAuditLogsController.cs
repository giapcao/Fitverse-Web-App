using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Application.AdminAuditLogs.Command;
using Application.Features;
using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SharedLibrary.Common;

namespace WebApi.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/admin-audit-logs/v{version:apiVersion}")]
public class AdminAuditLogsController : ApiController
{
    public AdminAuditLogsController(IMediator mediator) : base(mediator)
    {
    }

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<AdminAuditLogDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAuditLogs(CancellationToken ct)
    {
        var result = await _mediator.Send(new ListAdminAuditLogsQuery(), ct);
        if (result.IsFailure)
        {
            return HandleFailure(result);
        }

        return Ok(result.Value);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(AdminAuditLogDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetAuditLogById(Guid id, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetAdminAuditLogByIdQuery(id), ct);
        if (result.IsFailure)
        {
            return HandleFailure(result);
        }

        return Ok(result.Value);
    }

    [HttpPost]
    [ProducesResponseType(typeof(AdminAuditLogDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateAuditLog([FromBody] CreateAdminAuditLogCommand command, CancellationToken ct)
    {
        var result = await _mediator.Send(command, ct);
        if (result.IsFailure)
        {
            return HandleFailure(result);
        }

        var version = HttpContext.GetRequestedApiVersion()?.ToString() ?? "1.0";
        return CreatedAtAction(nameof(GetAuditLogById), new { id = result.Value.Id, version }, result.Value);
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(AdminAuditLogDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateAuditLog(Guid id, [FromBody] UpdateAdminAuditLogCommand command, CancellationToken ct)
    {
        var merged = command with { Id = id };
        var result = await _mediator.Send(merged, ct);
        if (result.IsFailure)
        {
            return HandleFailure(result);
        }

        return Ok(result.Value);
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteAuditLog(Guid id, CancellationToken ct)
    {
        var result = await _mediator.Send(new DeleteAdminAuditLogCommand(id), ct);
        if (result.IsFailure)
        {
            return HandleFailure(result);
        }

        return NoContent();
    }
}
