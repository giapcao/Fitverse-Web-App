using System;
using System.Threading;
using System.Threading.Tasks;
using Application.WalletLedgerEntries.Commands;
using Application.WalletLedgerEntries.Queries;
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
public class WalletLedgerEntriesController : ApiController
{
    public WalletLedgerEntriesController(IMediator mediator) : base(mediator)
    {
    }

    [HttpGet]
    [ProducesResponseType(typeof(WalletLedgerEntryResponse[]), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetWalletLedgerEntries([FromQuery] Guid? walletId, [FromQuery] Guid? journalId, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetWalletLedgerEntriesQuery(walletId, journalId), cancellationToken);
        if (result.IsFailure)
        {
            return HandleFailure(result);
        }

        return Ok(result.Value);
    }

    [HttpGet("wallets/{walletId:guid}")]
    [ProducesResponseType(typeof(WalletLedgerEntryResponse[]), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetWalletLedgerEntriesByWalletId(Guid walletId, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetWalletLedgerEntriesQuery(walletId, null), cancellationToken);
        if (result.IsFailure)
        {
            return HandleFailure(result);
        }

        return Ok(result.Value);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(WalletLedgerEntryResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetWalletLedgerEntryById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetWalletLedgerEntryByIdQuery(id), cancellationToken);
        if (result.IsFailure)
        {
            return HandleFailure(result);
        }

        return Ok(result.Value);
    }

    [HttpPost]
    [ProducesResponseType(typeof(WalletLedgerEntryResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateWalletLedgerEntry([FromBody] CreateWalletLedgerEntryCommand command, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command, cancellationToken);
        if (result.IsFailure)
        {
            return HandleFailure(result);
        }

        var version = HttpContext.GetRequestedApiVersion()?.ToString() ?? "1.0";
        return CreatedAtAction(nameof(GetWalletLedgerEntryById), new { id = result.Value.Id, version }, result.Value);
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(WalletLedgerEntryResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateWalletLedgerEntry(Guid id, [FromBody] UpdateWalletLedgerEntryCommand command, CancellationToken cancellationToken)
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
    public async Task<IActionResult> DeleteWalletLedgerEntry(Guid id, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new DeleteWalletLedgerEntryCommand(id), cancellationToken);
        if (result.IsFailure)
        {
            return HandleFailure(result);
        }

        return NoContent();
    }
}
