using System;
using System.Linq;
using Application.Payments.Returns;
using Application.Payments.VNPay;
using Application.Payments.VNPay.Queries;
using Application.VNPay.Commands;
using Application.VNPay.Queries;
using Asp.Versioning;
using Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using SharedLibrary.Common.ResponseModel;
using WebApi.Options;

namespace WebApi.Controllers;

[AllowAnonymous]
[ApiController]
[ApiVersion("1.0")]
[Route("api/vnpay/v{version:apiVersion}")]
public sealed class VnPayController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IOptions<VNPayOptions> _options;

    public VnPayController(IMediator mediator, IOptions<VNPayOptions> options)
    {
        _mediator = mediator;
        _options = options;
    }

    [HttpGet("checkout/vnpay")]
    public async Task<IActionResult> StartCheckout([FromQuery] long amountVnd, [FromQuery] string orderId, [FromQuery] Guid? walletId,[FromQuery] Guid userId, CancellationToken cancellationToken)
    {
        var config = BuildConfiguration();
        var clientIp = ResolveClientIp();

        var query = new GetVnPayCheckoutUrlQuery(
            amountVnd,
            orderId,
            clientIp,
            walletId,
            userId,
            config,
            DateTime.UtcNow);

        var result = await _mediator.Send(query, cancellationToken);
        if (result.IsFailure)
        {
            return CreateErrorResponse(result.Error);
        }

        return Ok(new
        {
            url = result.Value
        });
    }

    [HttpGet("vnpay/return")]
    public async Task<IActionResult> Return(CancellationToken cancellationToken)
    {
        var config = BuildConfiguration();
        var parameters = Request.Query.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.ToString(), StringComparer.Ordinal);
        var userId = parameters.TryGetValue("userId", out var userIdValue) && Guid.TryParse(userIdValue, out var parsedUserId)
            ? parsedUserId
            : (Guid?)null;

        var command = new ProcessPaymentGatewayReturnCommand(Gateway.Vnpay, parameters, config, userId);
        var commandResult = await _mediator.Send(command, cancellationToken);
        if (commandResult.IsFailure)
        {
            return CreateErrorResponse(commandResult.Error);
        }

        if (commandResult.Value.UserId.HasValue)
        {
            parameters["userId"] = commandResult.Value.UserId.Value.ToString();
        }

        var query = new GetVNPayReturnViewQuery(parameters, config, commandResult.Value.UserId);
        var result = await _mediator.Send(query, cancellationToken);
        if (result.IsFailure)
        {
            return CreateErrorResponse(result.Error);
        }

        return Content(result.Value.HtmlContent, "text/html");
    }

    [HttpGet("vnpay/ipn")]
    public async Task<IActionResult> Ipn(CancellationToken cancellationToken)
    {
        var config = BuildConfiguration();
        var parameters = Request.Query.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.ToString(), StringComparer.Ordinal);

        var command = new ProcessVnPayIpnCommand(parameters, config);
        var result = await _mediator.Send(command, cancellationToken);
        if (result.IsFailure)
        {
            return CreateErrorResponse(result.Error);
        }

        return new JsonResult(result.Value);
    }

    private VNPayConfiguration BuildConfiguration()
    {
        var value = _options.Value;
        return new VNPayConfiguration(
            value.TmnCode ?? string.Empty,
            value.HashSecret ?? string.Empty,
            value.BaseUrl ?? string.Empty,
            value.ReturnUrl ?? string.Empty,
            value.IpnUrl ?? string.Empty);
    }

    private string ResolveClientIp()
    {
        if (Request.Headers.TryGetValue("X-Forwarded-For", out var forwarded))
        {
            var realIp = forwarded
                .ToString()
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .FirstOrDefault();
            if (!string.IsNullOrWhiteSpace(realIp))
            {
                return realIp;
            }
        }

        var remoteIp = HttpContext.Connection.RemoteIpAddress;
        if (remoteIp != null)
        {
            if (remoteIp.IsIPv4MappedToIPv6)
                remoteIp = remoteIp.MapToIPv4();

            return remoteIp.ToString();
        }

        return "127.0.0.1";
    }


    private IActionResult CreateErrorResponse(Error error)
    {
        var statusCode = error.Code == VnPayErrors.ConfigurationMissing.Code
            ? StatusCodes.Status500InternalServerError
            : StatusCodes.Status400BadRequest;

        return StatusCode(statusCode, new { error.Code, error.Description });
    }
}
