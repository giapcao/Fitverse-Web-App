using Application.Reviews.Command;
using Application.Reviews.Query;
using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using SharedLibrary.Common;
using WebApi.Contracts.Reviews;

namespace WebApi.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/[controller]/v{version:apiVersion}")]
public class ReviewsController : ApiController
{
    public ReviewsController(IMediator mediator)
        : base(mediator)
    {
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateReviewRequest request, CancellationToken cancellationToken)
    {
        var command = new CreateReviewCommand(
            request.BookingId,
            request.UserId,
            request.CoachId,
            request.Rating,
            request.Comment,
            request.IsPublic);

        var result = await _mediator.Send(command, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : HandleFailure(result);
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var query = new GetAllReviewsQuery();
        var result = await _mediator.Send(query, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : HandleFailure(result);
    }

    [HttpGet("{bookingId:guid}")]
    public async Task<IActionResult> GetByBooking(Guid bookingId, CancellationToken cancellationToken)
    {
        var query = new GetReviewByBookingIdQuery(bookingId);
        var result = await _mediator.Send(query, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : HandleFailure(result);
    }

    [HttpGet("coach/{coachId:guid}")]
    public async Task<IActionResult> GetByCoach(Guid coachId, CancellationToken cancellationToken)
    {
        var query = new GetCoachReviewsQuery(coachId);
        var result = await _mediator.Send(query, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : HandleFailure(result);
    }
}
