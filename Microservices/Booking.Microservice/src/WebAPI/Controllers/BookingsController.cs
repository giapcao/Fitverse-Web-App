using System;
using System.Collections.Generic;
using System.Threading;
using System.Linq;
using System.Threading.Tasks;
using Application.Bookings.Commands;
using Application.Bookings.Queries;
using Application.Features;
using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SharedLibrary.Common;
using SharedLibrary.Common.ResponseModel;

namespace WebAPI.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/[controller]/v{version:apiVersion}")]
public class BookingsController : ApiController
{
    public BookingsController(IMediator mediator) : base(mediator)
    {
    }

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyCollection<BookingDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetBookings([FromQuery] Guid? userId, [FromQuery] Guid? coachId, CancellationToken cancellationToken)
    {
        Result<IReadOnlyCollection<BookingDto>> result;
        if (userId.HasValue)
        {
            result = await _mediator.Send(new ListBookingsByUserQuery(userId.Value), cancellationToken);
        }
        else if (coachId.HasValue)
        {
            result = await _mediator.Send(new ListBookingsByCoachQuery(coachId.Value), cancellationToken);
        }
        else
        {
            result = await _mediator.Send(new ListBookingsQuery(), cancellationToken);
        }

        if (result.IsFailure)
        {
            return HandleFailure(result);
        }

        return Ok(result.Value);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(BookingDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetBookingById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetBookingByIdQuery(id), cancellationToken);
        if (result.IsFailure)
        {
            return HandleFailure(result);
        }

        return Ok(result.Value);
    }

    [HttpPost]
    [ProducesResponseType(typeof(BookingDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateBooking([FromBody] CreateBookingCommand command, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command, cancellationToken);
        if (result.IsFailure)
        {
            return HandleFailure(result);
        }

        var version = HttpContext.GetRequestedApiVersion()?.ToString() ?? "1.0";
        return CreatedAtAction(nameof(GetBookingById), new { id = result.Value.Id, version }, result.Value);
    }

    [HttpPost("pending-package")]
    [ProducesResponseType(typeof(BookingDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreatePendingSubscriptionBooking(
        [FromBody] CreatePendingSubscriptionBookingCommand command,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command, cancellationToken);
        if (result.IsFailure)
        {
            return HandleFailure(result);
        }

        var version = HttpContext.GetRequestedApiVersion()?.ToString() ?? "1.0";
        return CreatedAtAction(nameof(GetBookingById), new { id = result.Value.Id, version }, result.Value);
    }

    [HttpPost("subscription-package")]
    [ProducesResponseType(typeof(IReadOnlyCollection<BookingDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateSubscriptionPackageBookings(
        [FromBody] CreateSubscriptionPackageBookingsCommand command,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command, cancellationToken);
        if (result.IsFailure)
        {
            return HandleFailure(result);
        }

        var firstBooking = result.Value.First();
        var version = HttpContext.GetRequestedApiVersion()?.ToString() ?? "1.0";
        return CreatedAtAction(nameof(GetBookingById), new { id = firstBooking.Id, version }, result.Value);
    }

    [HttpPost("pending-package/cancel")]
    [ProducesResponseType(typeof(BookingDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CancelPendingSubscriptionBooking(
        [FromBody] CancelPendingSubscriptionBookingCommand command,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command, cancellationToken);
        if (result.IsFailure)
        {
            return HandleFailure(result);
        }

        return Ok(result.Value);
    }

    [HttpPost("pending-package/confirm")]
    [ProducesResponseType(typeof(BookingDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ConfirmPendingSubscriptionBooking(
        [FromBody] ConfirmPendingSubscriptionBookingCommand command,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command, cancellationToken);
        if (result.IsFailure)
        {
            return HandleFailure(result);
        }

        return Ok(result.Value);
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(BookingDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateBooking(Guid id, [FromBody] UpdateBookingCommand command, CancellationToken cancellationToken)
    {
        if (command.Id != id)
        {
            return BadRequest("Route id and payload id must match.");
        }

        var result = await _mediator.Send(command, cancellationToken);
        if (result.IsFailure)
        {
            return HandleFailure(result);
        }

        return Ok(result.Value);
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteBooking(Guid id, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new DeleteBookingCommand(id), cancellationToken);
        if (result.IsFailure)
        {
            return HandleFailure(result);
        }

        return NoContent();
    }
}
