using System;
using System.Threading;
using System.Threading.Tasks;
using Application.CoachProfiles.Command;
using Application.CoachProfiles.Query;
using Application.Features;
using Application.KycRecords.Command;
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
public class CoachProfilesController : ApiController
{
    public CoachProfilesController(IMediator mediator) : base(mediator)
    {
    }

    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<CoachProfileDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetProfiles(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(new ListCoachProfilesQuery(pageNumber, pageSize), cancellationToken);
        if (result.IsFailure)
        {
            return HandleFailure(result);
        }

        return Ok(result.Value);
    }

    [HttpGet("{coachId:guid}")]
    [ProducesResponseType(typeof(CoachProfileDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetProfileById(Guid coachId, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetCoachProfileByIdQuery(coachId), cancellationToken);
        if (result.IsFailure)
        {
            return HandleFailure(result);
        }

        return Ok(result.Value);
    }

    [HttpPost]
    [ProducesResponseType(typeof(CoachProfileDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateProfile([FromBody] CreateCoachProfileCommand command, CancellationToken cancellationToken)
    {
        var profileResult = await _mediator.Send(command, cancellationToken);
        if (profileResult.IsFailure)
        {
            return HandleFailure(profileResult);
        }

        var profile = profileResult.Value;

        var kycResult = await _mediator.Send(
            new CreateKycRecordCommand(profile.CoachId, null, "kyc_coach_profile"),
            cancellationToken);
        if (kycResult.IsFailure)
        {
            return HandleFailure(kycResult);
        }

        var version = HttpContext.GetRequestedApiVersion()?.ToString() ?? "1.0";
        return CreatedAtAction(nameof(GetProfileById), new { coachId = profile.CoachId, version }, profile);
    }

    [HttpPut("{coachId:guid}")]
    [ProducesResponseType(typeof(CoachProfileDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateProfile(Guid coachId, [FromBody] UpdateCoachProfileCommand command, CancellationToken cancellationToken)
    {
        var merged = command with { CoachId = coachId };
        var result = await _mediator.Send(merged, cancellationToken);
        if (result.IsFailure)
        {
            return HandleFailure(result);
        }

        return Ok(result.Value);
    }

    [HttpDelete("{coachId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteProfile(Guid coachId, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new DeleteCoachProfileCommand(coachId), cancellationToken);
        if (result.IsFailure)
        {
            return HandleFailure(result);
        }

        return NoContent();
    }
}
