using System;
using System.Threading;
using System.Threading.Tasks;
using Application.WalletJournals.Commands;
using Application.WalletJournals.Queries;
using Application.Wallets.Queries;
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
public class WalletJournalsController : ApiController
{
    public WalletJournalsController(IMediator mediator) : base(mediator)
    {
    }

    [HttpGet]
    [ProducesResponseType(typeof(WalletJournalResponse[]), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetWalletJournals([FromQuery] Guid? bookingId, [FromQuery] Guid? paymentId, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetWalletJournalsQuery(bookingId, paymentId), cancellationToken);
        if (result.IsFailure)
        {
            return HandleFailure(result);
        }

        return Ok(result.Value);
    }

    [HttpGet("wallets/{walletId:guid}")]
    [ProducesResponseType(typeof(WalletJournalResponse[]), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetWalletJournalsByWalletId(Guid walletId, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetWalletJournalsByWalletIdQuery(walletId), cancellationToken);
        if (result.IsFailure)
        {
            return HandleFailure(result);
        }

        return Ok(result.Value);
    }

    [HttpGet("wallets/{walletId:guid}/details")]
    [ProducesResponseType(typeof(WalletHistoryResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetWalletHistoryByWalletId(Guid walletId, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetWalletHistoryByWalletIdQuery(walletId), cancellationToken);
        if (result.IsFailure)
        {
            return HandleFailure(result);
        }

        return Ok(result.Value);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(WalletJournalResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetWalletJournalById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetWalletJournalByIdQuery(id), cancellationToken);
        if (result.IsFailure)
        {
            return HandleFailure(result);
        }

        return Ok(result.Value);
    }

    [HttpPost]
    [ProducesResponseType(typeof(WalletJournalResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateWalletJournal([FromBody] CreateWalletJournalCommand command, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command, cancellationToken);
        if (result.IsFailure)
        {
            return HandleFailure(result);
        }

        var version = HttpContext.GetRequestedApiVersion()?.ToString() ?? "1.0";
        return CreatedAtAction(nameof(GetWalletJournalById), new { id = result.Value.Id, version }, result.Value);
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(WalletJournalResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateWalletJournal(Guid id, [FromBody] UpdateWalletJournalCommand command, CancellationToken cancellationToken)
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
    public async Task<IActionResult> DeleteWalletJournal(Guid id, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new DeleteWalletJournalCommand(id), cancellationToken);
        if (result.IsFailure)
        {
            return HandleFailure(result);
        }

        return NoContent();
    }
}
