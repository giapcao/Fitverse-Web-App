using System;
using System.Threading;
using System.Threading.Tasks;
using Application.WalletBalances.Commands;
using Application.WalletBalances.Queries;
using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SharedLibrary.Common;

namespace WebApi.Controllers;

[Authorize]
[ApiController]
[ApiVersion("1.0")]
[Route("api/[controller]/v{version:apiVersion}")]
public class WalletBalancesController : ApiController
{
    public WalletBalancesController(IMediator mediator) : base(mediator)
    {
    }

    [HttpGet]
    [ProducesResponseType(typeof(WalletBalanceResponse[]), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetWalletBalances([FromQuery] Guid? walletId, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetWalletBalancesQuery(walletId), cancellationToken);
        if (result.IsFailure)
        {
            return HandleFailure(result);
        }

        return Ok(result.Value);
    }

    [HttpGet("users/{userId:guid}")]
    [ProducesResponseType(typeof(WalletBalanceResponse[]), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetWalletBalancesByUserId(Guid userId, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetWalletBalancesByUserIdQuery(userId), cancellationToken);
        if (result.IsFailure)
        {
            return HandleFailure(result);
        }

        return Ok(result.Value);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(WalletBalanceResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetWalletBalanceById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetWalletBalanceByIdQuery(id), cancellationToken);
        if (result.IsFailure)
        {
            return HandleFailure(result);
        }

        return Ok(result.Value);
    }

    [HttpPost]
    [ProducesResponseType(typeof(WalletBalanceResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateWalletBalance([FromBody] CreateWalletBalanceCommand command, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command, cancellationToken);
        if (result.IsFailure)
        {
            return HandleFailure(result);
        }

        var version = HttpContext.GetRequestedApiVersion()?.ToString() ?? "1.0";
        return CreatedAtAction(nameof(GetWalletBalanceById), new { id = result.Value.Id, version }, result.Value);
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(WalletBalanceResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateWalletBalance(Guid id, [FromBody] UpdateWalletBalanceCommand command, CancellationToken cancellationToken)
    {
        var merged = command with { Id = id };
        var result = await _mediator.Send(merged, cancellationToken);
        if (result.IsFailure)
        {
            return HandleFailure(result);
        }

        return Ok(result.Value);
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteWalletBalance(Guid id, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new DeleteWalletBalanceCommand(id), cancellationToken);
        if (result.IsFailure)
        {
            return HandleFailure(result);
        }

        return NoContent();
    }
}
