using System;
using System.Text.Json;
using Domain.Enums;

namespace Application.Payments.Queries;

public sealed record PaymentResponse(
    Guid Id,
    Guid BookingId,
    long AmountVnd,
    string? GatewayTxnId,
    JsonDocument? GatewayMeta,
    DateTime? PaidAt,
    long? RefundAmountVnd,
    Gateway Gateway,
    PaymentStatus Status,
    DateTime CreatedAt);
