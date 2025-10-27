using System;
using System.Collections.Concurrent;
using Application.Features;

namespace Application.Common.Stores;

public interface IPendingPackagePaymentStatusStore
{
    void Set(Guid bookingId, PendingPackagePaymentStatusDto status);

    bool TryGet(Guid bookingId, out PendingPackagePaymentStatusDto status);
}

public sealed class PendingPackagePaymentStatusStore : IPendingPackagePaymentStatusStore
{
    private readonly ConcurrentDictionary<Guid, PendingPackagePaymentStatusDto> _store = new();

    public void Set(Guid bookingId, PendingPackagePaymentStatusDto status)
    {
        _store[bookingId] = status;
    }

    public bool TryGet(Guid bookingId, out PendingPackagePaymentStatusDto status) =>
        _store.TryGetValue(bookingId, out status);
}
