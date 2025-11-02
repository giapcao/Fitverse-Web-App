using Application.Options;
using Application.Payments.Commands;
using Application.Payments.Models;
using Application.Payments.Queries;
using Application.Payments.Returns;
using Application.Payments.VNPay.Queries;
using Application.VNPay;
using Application.VNPay.Commands;
using Application.PayOs;
using Asp.Versioning;
using Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using SharedLibrary.Common;
using SharedLibrary.Common.ResponseModel;
using SharedLibrary.Contracts.Payments;
using WebApi.Helper;

namespace WebApi.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/[controller]/v{version:apiVersion}")]
public class PaymentsController : ApiController
{
    private readonly IOptions<VNPayOptions> _vnPayOptions;
    private readonly IOptions<MomoOptions> _momoOptions;
    private readonly IOptions<PayOsOptions> _payOsOptions;

    public PaymentsController(
        IMediator mediator,
        IOptions<VNPayOptions> vnPayOptions,
        IOptions<MomoOptions> momoOptions,
        IOptions<PayOsOptions> payOsOptions) : base(mediator)
    {
        _vnPayOptions = vnPayOptions;
        _momoOptions = momoOptions;
        _payOsOptions = payOsOptions;
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

    [HttpGet("user/{userId:guid}")]
    [ProducesResponseType(typeof(PaymentResponse[]), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPaymentsByUserId(Guid userId, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetPaymentsByUserIdQuery(userId), cancellationToken);
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
    [ProducesResponseType(typeof(InitiatePaymentCombinedResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(InitiatePaymentCombinedResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> InitiatePayment(
        [FromBody] InitiatePaymentRequest request,
        CancellationToken cancellationToken)
    {
        var command = new InitiatePendingSubscriptionPaymentCommand(
            request.AmountVnd,
            request.Gateway,
            request.BookingId,
            request.Flow,
            request.UserId,
            request.WalletId,
            PaymentsHelpers.ResolveClientIp(HttpContext));

        var result = await _mediator.Send(command, cancellationToken);
        if (result.IsFailure)
        {
            return HandleFailure(result);
        }

        var response = result.Value;
        if (response.PaymentId.HasValue)
        {
            var version = HttpContext.GetRequestedApiVersion()?.ToString() ?? "1.0";
            return CreatedAtAction(
                nameof(GetPaymentById),
                new { id = response.PaymentId.Value, version },
                response);
        }

        return Ok(response);
    }

    [AllowAnonymous]
    [HttpGet("depositWallet")]
    public async Task<IActionResult> WalletReturn(CancellationToken cancellationToken)
    {
        var parameters = Request.Query.ToDictionary(
            kvp => kvp.Key,
            kvp => kvp.Value.ToString(),
            StringComparer.OrdinalIgnoreCase);

        if (!PaymentsHelpers.TryResolveGatewayContext(
                parameters,
                _vnPayOptions.Value,
                _momoOptions.Value,
                _payOsOptions.Value,
                out var gateway,
                out var configuration,
                out var userId,
                out var error))
        {
            return CreateErrorResponse(error ?? new Error("Payment.GatewayUnknown", "Unable to determine payment gateway from return payload."));
        }

        var command = new ProcessPaymentGatewayReturnCommand(gateway, parameters, configuration, userId);
        var commandResult = await _mediator.Send(command, cancellationToken);
        if (commandResult.IsFailure)
        {
            return CreateErrorResponse(commandResult.Error);
        }

        if (commandResult.Value.UserId.HasValue)
        {
            parameters["userId"] = commandResult.Value.UserId.Value.ToString();
        }

        switch (gateway)
        {
            case Gateway.Vnpay when configuration is VnPayConfiguration vnPayConfiguration:
            {
                var query = new GetVNPayReturnViewQuery(parameters, vnPayConfiguration, commandResult.Value.UserId);
                var result = await _mediator.Send(query, cancellationToken);
                if (result.IsFailure)
                {
                    return CreateErrorResponse(result.Error);
                }

                return Content(result.Value.HtmlContent, "text/html");
            }

            case Gateway.Momo:
            {
                var html = PaymentsHelpers.BuildMomoReturnHtml(parameters, commandResult.Value);
                return Content(html, "text/html");
            }

            case Gateway.Payos:
            {
                var html = PaymentsHelpers.BuildPayOsReturnHtml(parameters, commandResult.Value);
                return Content(html, "text/html");
            }

            default:
                return CreateErrorResponse(new Error(
                    "Payment.GatewayNotSupported",
                    $"Gateway '{gateway}' is not supported for wallet return."));
        }
    }

    [AllowAnonymous]
    [HttpGet("booking")]
    public async Task<IActionResult> BookingReturn(CancellationToken cancellationToken)
    {
        var parameters = Request.Query.ToDictionary(
            kvp => kvp.Key,
            kvp => kvp.Value.ToString(),
            StringComparer.OrdinalIgnoreCase);

        if (!PaymentsHelpers.TryResolveGatewayContext(
                parameters,
                _vnPayOptions.Value,
                _momoOptions.Value,
                _payOsOptions.Value,
                out var gateway,
                out var configuration,
                out var userId,
                out var error))
        {
            return CreateErrorResponse(error ?? new Error("Payment.GatewayUnknown", "Unable to determine payment gateway from return payload."));
        }

        var command = new ProcessPaymentGatewayReturnCommand(gateway, parameters, configuration, userId);
        var commandResult = await _mediator.Send(command, cancellationToken);
        if (commandResult.IsFailure)
        {
            return CreateErrorResponse(commandResult.Error);
        }

        if (commandResult.Value.UserId.HasValue)
        {
            parameters["userId"] = commandResult.Value.UserId.Value.ToString();
        }

        var paymentId = PaymentsHelpers.TryResolvePaymentId(parameters, out var resolvedPaymentId) ? resolvedPaymentId : (Guid?)null;
        var resolvedUserId = commandResult.Value.UserId ?? userId;

        return Ok(new
        {
            success = commandResult.Value.TransactionCaptured,
            paymentId,
            userId = resolvedUserId,
            gateway
        });
    }

    [AllowAnonymous]
    [HttpGet("ipn")]
    public async Task<IActionResult> Ipn(CancellationToken cancellationToken)
    {
        var config = PaymentsHelpers.BuildVnPayConfiguration(_vnPayOptions.Value);
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

    private IActionResult CreateErrorResponse(Error error)
    {
        var statusCode = PaymentsHelpers.ResolveErrorStatusCode(error);
        return StatusCode(statusCode, new { error.Code, error.Description });
    }
}
