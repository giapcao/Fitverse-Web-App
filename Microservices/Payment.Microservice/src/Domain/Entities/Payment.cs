using System.Text.Json;
using Domain.Enums;
using SharedLibrary.Contracts.Payments;

namespace Domain.Entities;

public partial class Payment
{
    public Guid Id { get; set; }

    public long AmountVnd { get; set; }

    public string? GatewayTxnId { get; set; }

    public JsonDocument? GatewayMeta { get; set; }

    public DateTime? PaidAt { get; set; }

    public long? RefundAmountVnd { get; set; }

    public Gateway Gateway { get; set; }

    public PaymentStatus Status { get; set; }

    public DateTime CreatedAt { get; set; }
}
