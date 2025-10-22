using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Application.Payments.Commands;
using Application.Payments.Queries;
using Application.Payments;
using Application.Payments.Returns;
using Application.Payments.VNPay;
using Application.Payments.VNPay.Queries;
using Application.VNPay;
using Application.VNPay.Commands;
using Application.VNPay.Queries;
using Asp.Versioning;
using Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using SharedLibrary.Common;
using SharedLibrary.Common.ResponseModel;
using WebApi.Options;

namespace WebApi.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/[controller]/v{version:apiVersion}")]
public class PaymentsController : ApiController
{
    private readonly IOptions<VNPayOptions> _vnPayOptions;

    public PaymentsController(IMediator mediator, IOptions<VNPayOptions> vnPayOptions) : base(mediator)
    {
        _vnPayOptions = vnPayOptions;
    }

    [HttpGet]
    [ProducesResponseType(typeof(PaymentResponse[]), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPayments(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetPaymentsQuery(), cancellationToken);
        if (result.IsFailure)
        {
            return HandleFailure(result);
        }

        return Ok(result.Value);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(PaymentResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetPaymentById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetPaymentByIdQuery(id), cancellationToken);
        if (result.IsFailure)
        {
            return HandleFailure(result);
        }

        return Ok(result.Value);
    }

    [HttpPost]
    [ProducesResponseType(typeof(PaymentResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreatePayment([FromBody] CreatePaymentCommand command,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command, cancellationToken);
        if (result.IsFailure)
        {
            return HandleFailure(result);
        }

        var version = HttpContext.GetRequestedApiVersion()?.ToString() ?? "1.0";
        return CreatedAtAction(nameof(GetPaymentById), new { id = result.Value.Id, version }, result.Value);
    }

    [HttpPost("initiate")]
    [ProducesResponseType(typeof(InitiatePaymentResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(InitiatePaymentResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> InitiatePayment([FromBody] InitiatePaymentCommand command,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command, cancellationToken);
        if (result.IsFailure)
        {
            return HandleFailure(result);
        }

        var version = HttpContext.GetRequestedApiVersion()?.ToString() ?? "1.0";
        if (result.Value.PaymentId.HasValue)
        {
            return CreatedAtAction(
                nameof(GetPaymentById),
                new { id = result.Value.PaymentId.Value, version },
                result.Value);
        }

        return Ok(result.Value);
    }

    [HttpPost("bookingWallet")]
    public async Task<IActionResult> BookingWallet(
        [FromBody] CaptureBookingWalletPaymentCommand request,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(request, cancellationToken);
        if (result.IsFailure)
        {
            return HandleFailure(result);
        }

        return Ok(new { success = true });
    }

    [AllowAnonymous]
    [HttpGet("checkout")]
    public async Task<IActionResult> StartCheckout(
        [FromQuery] long amountVnd,
        [FromQuery] string orderId,
        [FromQuery] Guid? walletId,
        [FromQuery] Guid userId,
        [FromQuery] PaymentFlow? flow,
        CancellationToken cancellationToken)
    {
        var config = BuildVnPayConfiguration();
        var clientIp = ResolveClientIp();
        var selectedFlow = flow ?? PaymentFlow.DepositWallet;

        var query = new GetVnPayCheckoutUrlQuery(
            amountVnd,
            orderId,
            clientIp,
            walletId,
            userId,
            selectedFlow,
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

    [AllowAnonymous]
    [HttpGet("depositWallet")]
    public async Task<IActionResult> WalletReturn(CancellationToken cancellationToken)
    {
        var config = BuildVnPayConfiguration();
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

    [AllowAnonymous]
    [HttpGet("booking")]
    public async Task<IActionResult> BookingReturn(CancellationToken cancellationToken)
    {
        var config = BuildVnPayConfiguration();
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

        Guid? paymentId = null;
        if (TryResolvePaymentId(parameters, out var resolvedPaymentId))
        {
            paymentId = resolvedPaymentId;
        }

        return Ok(new
        {
            success = true,
            paymentId,
            userId = commandResult.Value.UserId ?? userId
        });
    }

    [AllowAnonymous]
    [HttpGet("ipn")]
    public async Task<IActionResult> Ipn(CancellationToken cancellationToken)
    {
        var config = BuildVnPayConfiguration();
        var parameters = Request.Query.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.ToString(), StringComparer.Ordinal);

        var command = new ProcessVnPayIpnCommand(parameters, config);
        var result = await _mediator.Send(command, cancellationToken);
        if (result.IsFailure)
        {
            return CreateErrorResponse(result.Error);
        }

        return new JsonResult(result.Value);
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(PaymentResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdatePayment(Guid id, [FromBody] UpdatePaymentCommand command,
        CancellationToken cancellationToken)
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
    public async Task<IActionResult> DeletePayment(Guid id, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new DeletePaymentCommand(id), cancellationToken);
        if (result.IsFailure)
        {
            return HandleFailure(result);
        }

        return NoContent();
    }

    private VnPayConfiguration BuildVnPayConfiguration()
    {
        var value = _vnPayOptions.Value;
        return new VnPayConfiguration(
            value.TmnCode,
            value.HashSecret,
            value.BaseUrl,
            value.ReturnWalletUrl,
            value.ReturnBookingUrl,
            value.IpnUrl);
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
            {
                remoteIp = remoteIp.MapToIPv4();
            }

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

    private static bool TryResolvePaymentId(IReadOnlyDictionary<string, string> parameters, out Guid paymentId)
    {
        if (parameters.TryGetValue("vnp_TxnRef", out var txnRef) && Guid.TryParse(txnRef, out paymentId))
        {
            return true;
        }

        if (parameters.TryGetValue("vnp_OrderInfo", out var orderInfo))
        {
            var trailing = ExtractTrailingToken(orderInfo);
            if (!string.IsNullOrWhiteSpace(trailing) && Guid.TryParse(trailing, out paymentId))
            {
                return true;
            }
        }

        paymentId = Guid.Empty;
        return false;
    }

    private static string? ExtractTrailingToken(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        return value
            .Split(':', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .LastOrDefault();
    }
}
