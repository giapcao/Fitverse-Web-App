using System;
using MassTransit;

namespace Application.Sagas;

public class PendingSubscriptionPackageSagaData : SagaStateMachineInstance, ISagaVersion
{
    public Guid CorrelationId { get; set; }
    public string CurrentState { get; set; } = default!;
    public Guid SubscriptionId { get; set; }
    public Guid BookingId { get; set; }
    public Guid? WalletId { get; set; }
    public Guid? PaymentId { get; set; }
    public Guid? WalletJournalId { get; set; }
    public bool WalletCaptured { get; set; }
    public string Status { get; set; } = "Pending";
    public string? FailureCode { get; set; }
    public string? FailureReason { get; set; }
    public DateTime StartedAtUtc { get; set; }
    public DateTime? CompletedAtUtc { get; set; }
    public int Version { get; set; }
}

