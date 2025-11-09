using Application.Conversations.Command;
using Application.Conversations.Query;
using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using SharedLibrary.Common;
using WebApi.Contracts.Conversations;

namespace WebApi.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/[controller]/v{version:apiVersion}")]
public class ConversationsController : ApiController
{
    public ConversationsController(IMediator mediator)
        : base(mediator)
    {
    }

    [HttpPost("messages")]
    public async Task<IActionResult> SendMessage([FromBody] SendMessageRequest request, CancellationToken cancellationToken)
    {
        var command = new SendMessageCommand(
            request.UserId,
            request.CoachId,
            request.SenderId,
            request.Body,
            request.AttachmentUrl);

        var result = await _mediator.Send(command, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : HandleFailure(result);
    }

    [HttpGet("thread")]
    public async Task<IActionResult> GetThread([FromQuery] Guid userId, [FromQuery] Guid coachId, CancellationToken cancellationToken)
    {
        var query = new GetConversationByParticipantsQuery(userId, coachId);
        var result = await _mediator.Send(query, cancellationToken);

        return result.IsSuccess ? Ok(result.Value) : HandleFailure(result);
    }
}
