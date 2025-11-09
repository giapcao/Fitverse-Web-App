using Application.Disputes.Command;
using Application.Disputes.Query;
using Domain.Persistence.Enums;
using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using SharedLibrary.Common;
using WebApi.Contracts.Disputes;

namespace WebApi.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/[controller]/v{version:apiVersion}")]
public class DisputesController : ApiController
{
    public DisputesController(IMediator mediator)
        : base(mediator)
    {
    }

    [HttpPost]
    public async Task<IActionResult> Open([FromBody] OpenDisputeRequest request, CancellationToken cancellationToken)
    {
        var command = new OpenDisputeTicketCommand(
            request.BookingId,
            request.OpenedBy,
            request.ReasonType,
            request.Description,
            request.EvidenceUrls);

        var result = await _mediator.Send(command, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : HandleFailure(result);
    }

    [HttpPost("{disputeId:guid}/resolve")]
    public async Task<IActionResult> Resolve(Guid disputeId, [FromBody] ResolveDisputeRequest request, CancellationToken cancellationToken)
    {
        var command = new ResolveDisputeTicketCommand(disputeId, request.ResolvedBy, request.Status, request.ResolutionNotes);
        var result = await _mediator.Send(command, cancellationToken);

        return result.IsSuccess ? Ok(result.Value) : HandleFailure(result);
    }

    [HttpGet]
    public async Task<IActionResult> List([FromQuery] DisputeStatus? status, CancellationToken cancellationToken)
    {
        var query = new ListDisputeTicketsQuery(status);
        var result = await _mediator.Send(query, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : HandleFailure(result);
    }
}
