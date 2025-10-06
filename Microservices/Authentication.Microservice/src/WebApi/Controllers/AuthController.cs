using Application.Authentication.Command;
using Application.Authentication.Query;
using Application.Authentication.Validator;
using Application.Features;
using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SharedLibrary.Common;
using SharedLibrary.Common.ResponseModel;

namespace WebApi.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/[controller]/v{version:apiVersion}")]
public class AuthController : ApiController
{
    public AuthController(IMediator mediator) : base(mediator)
    {
    }

    [AllowAnonymous]
    [HttpGet("google/url")]
    [ProducesResponseType(typeof(Result<AuthDto.GoogleAuthUrlDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetGoogleAuthorizationUrl([FromQuery] string? redirectUri, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetGoogleAuthUrlQuery(redirectUri), ct);
        if (result.IsFailure) return HandleFailure(result);
        return Ok(result);
    }

    [AllowAnonymous]
    [HttpGet("google/callback")]
    [ProducesResponseType(typeof(Result<AuthDto.LoginResultDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GoogleCallback([FromQuery] string code, [FromQuery] string state, [FromQuery] string? redirectUri, CancellationToken ct)
    {
        var result = await _mediator.Send(new LoginWithGoogleCommand(code, state, redirectUri), ct);
        if (result.IsFailure) return HandleFailure(result);
        return Ok(result);
    }

    [AllowAnonymous]
    [HttpPost("google/verify-id-token")]
    [ProducesResponseType(typeof(Result<AuthDto.LoginResultDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> VerifyGoogleIdToken([FromBody] AuthDto.GoogleIdTokenRequestDto request, CancellationToken ct)
    {
        var result = await _mediator.Send(new LoginWithGoogleViaIdTokenCommand(request.IdToken), ct);
        if (result.IsFailure) return HandleFailure(result);
        return Ok(result);
    }

    [AllowAnonymous]
    [HttpPost("verify-email")]
    [ProducesResponseType(typeof(Result<AuthDto.VerifyEmailResultDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> VerifyEmail([FromBody] VerifyEmailCommand request, CancellationToken ct)
    {
        var result = await _mediator.Send(request, ct);
        if (result.IsFailure) return HandleFailure(result);
        return Ok(result);
    }

    [AllowAnonymous]
    [HttpGet("verify-email")]
    [ProducesResponseType(typeof(Result<AuthDto.VerifyEmailResultDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> VerifyEmailGet([FromQuery] string token, CancellationToken ct)
    {
        var result = await _mediator.Send(new VerifyEmailCommand(token), ct);
        if (result.IsFailure) return HandleFailure(result);
        return Ok(result);
    }

    [AllowAnonymous]
    [HttpPost("login")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Login([FromBody] LoginCommand request, CancellationToken ct)
    {
        var result = await _mediator.Send(request, ct);
        if (result.IsFailure) return HandleFailure(result);
        return Ok(result);
    }

    [AllowAnonymous]
    [HttpPost("login/google")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> LoginWithGoogle([FromBody] AuthDto.GoogleLoginRequestDto request, CancellationToken ct)
    {
        var result = await _mediator.Send(new LoginWithGoogleCommand(request.Code, request.State, request.RedirectUri), ct);
        if (result.IsFailure) return HandleFailure(result);
        return Ok(result);
    }

    [AllowAnonymous]
    [HttpPost("register")]
    [ProducesResponseType(typeof(Result<Unit>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Register([FromBody] RegisterCommand request, CancellationToken ct)
    {
        var result = await _mediator.Send(request, ct);
        if (result.IsFailure) return HandleFailure(result);
        return Ok(result);
    }

    [AllowAnonymous]
    [HttpPost("refresh")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshCommand request, CancellationToken ct)
    {
        var result = await _mediator.Send(request, ct);
        if (result.IsFailure) return HandleFailure(result);
        return Ok(result);
    }

    [Authorize]
    [HttpPost("logout")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Logout(CancellationToken ct)
    {
        var result = await _mediator.Send(new LogoutCommand(), ct);
        if (result.IsFailure) return HandleFailure(result);
        return NoContent();
    }

    [AllowAnonymous]
    [HttpPost("forgot-password/request-otp")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> RequestOtp([FromBody] AuthDto.RequestOtpDto dto, CancellationToken ct)
    {
        var result = await _mediator.Send(new RequestPasswordOtpCommand(dto.Email), ct);
        if (result.IsFailure) return HandleFailure(result);
        return Ok(result);
    }

    [AllowAnonymous]
    [HttpPost("forgot-password/verify-otp-password")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> VerifyOtp([FromBody] AuthDto.VerifyOtpDto dto, CancellationToken ct)
    {
        var result = await _mediator.Send(new VerifyPasswordOtpCommand(dto.Email, dto.Otp, dto.NewPassword), ct);
        if (result.IsFailure) return HandleFailure(result);
        return Ok(result);
    }

    [AllowAnonymous]
    [HttpPost("forgot-password/verify-only-otp")]
    public async Task<IActionResult> VerifyOtpOnly([FromBody] AuthDto.VerifyOtpOnlyDto dto, CancellationToken ct)
    {
        var result = await _mediator.Send(new VerifyPasswordOtpOnlyCommand(dto.Email, dto.Otp), ct);
        if (result.IsFailure) return HandleFailure(result);
        return Ok(result);
    }

    [AllowAnonymous]
    [HttpPost("forgot-password/reset")]
    public async Task<IActionResult> ResetWithToken([FromBody] AuthDto.ResetWithTokenDto dto, CancellationToken ct)
    {
        var result = await _mediator.Send(new ResetPasswordWithTokenCommand(dto.ResetToken, dto.NewPassword), ct);
        if (result.IsFailure) return HandleFailure(result);
        return Ok(result);
    }
}