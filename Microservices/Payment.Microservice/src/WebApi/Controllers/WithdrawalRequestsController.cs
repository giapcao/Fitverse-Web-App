using System.Collections.Generic;
using Application.WithdrawalRequests.Commands;
using Application.WithdrawalRequests.Queries;
using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using SharedLibrary.Common;

namespace WebApi.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/[controller]/v{version:apiVersion}")]
public class WithdrawalRequestsController : ApiController
{
    public WithdrawalRequestsController(IMediator mediator) : base(mediator)
    {
    }

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<WithdrawalRequestResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetWithdrawalRequests(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetWithdrawalRequestsQuery(), cancellationToken);
        if (result.IsFailure)
        {
            return HandleFailure(result);
        }

        return Ok(result.Value);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(WithdrawalRequestResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetWithdrawalRequest(Guid id, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetWithdrawalRequestByIdQuery(id), cancellationToken);
        if (result.IsFailure)
        {
            return HandleFailure(result);
        }

        return Ok(result.Value);
    }

    [HttpPost]
    [ProducesResponseType(typeof(WithdrawalRequestResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateWithdrawalRequest(
        [FromBody] CreateWithdrawalRequestCommand command,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command, cancellationToken);
        if (result.IsFailure)
        {
            return HandleFailure(result);
        }

        var version = HttpContext.GetRequestedApiVersion()?.ToString() ?? "1.0";
        return CreatedAtAction(
            nameof(GetWithdrawalRequest),
            new { id = result.Value.Id, version },
            result.Value);
    }

    [HttpPut("{id:guid}/status")]
    [ProducesResponseType(typeof(WithdrawalRequestResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateWithdrawalRequestStatus(
        Guid id,
        [FromBody] UpdateWithdrawalRequestStatusCommand command,
        CancellationToken cancellationToken)
    {
        var merged = command with { Id = id };
        var result = await _mediator.Send(merged, cancellationToken);
        if (result.IsFailure)
        {
            return HandleFailure(result);
        }

        return Ok(result.Value);
    }
}
