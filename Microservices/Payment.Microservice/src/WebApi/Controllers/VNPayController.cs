using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Application.Payments.VNPay;
using Application.Payments.VNPay.Commands;
using Application.Payments.VNPay.Queries;
using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using SharedLibrary.Common.ResponseModel;
using WebApi.Constants;
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
    public async Task<IActionResult> StartCheckout([FromQuery] long amountVnd, [FromQuery] string orderId, CancellationToken cancellationToken)
    {
        var config = BuildConfiguration();
        var clientIp = ResolveClientIp();

        var query = new GetVNPayCheckoutUrlQuery(
            amountVnd,
            orderId,
            clientIp,
            config,
            DateTime.UtcNow);

        var result = await _mediator.Send(query, cancellationToken);
        if (result.IsFailure)
        {
            return CreateErrorResponse(result.Error);
        }

        return Redirect(result.Value);
    }

    [HttpGet("vnpay/return")]
    public async Task<IActionResult> Return(CancellationToken cancellationToken)
    {
        var config = BuildConfiguration();
        var parameters = Request.Query.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.ToString(), StringComparer.Ordinal);

        var query = new GetVNPayReturnViewQuery(parameters, config);
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
        var forwardedFor = Request.Headers["X-Forwarded-For"].ToString();
        if (!string.IsNullOrWhiteSpace(forwardedFor))
        {
            var forwardedIp = forwardedFor.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .FirstOrDefault();
            if (!string.IsNullOrWhiteSpace(forwardedIp))
            {
                return forwardedIp;
            }
        }

        return HttpContext.Connection.RemoteIpAddress?.ToString() ?? "127.0.0.1";
    }

    private IActionResult CreateErrorResponse(Error error)
    {
        var statusCode = error.Code == VnPayErrors.ConfigurationMissing.Code
            ? StatusCodes.Status500InternalServerError
            : StatusCodes.Status400BadRequest;

        return StatusCode(statusCode, new { error.Code, error.Description });
    }
}
