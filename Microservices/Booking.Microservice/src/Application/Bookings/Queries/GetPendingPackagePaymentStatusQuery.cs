using Application.Abstractions.Messaging;
using Application.Common.Stores;
using Application.Features;
using SharedLibrary.Common;
using SharedLibrary.Common.ResponseModel;

namespace Application.Bookings.Queries;

public sealed record GetPendingPackagePaymentStatusQuery(Guid BookingId)
    : IQuery<PendingPackagePaymentStatusDto>;

internal sealed class GetPendingPackagePaymentStatusQueryHandler
    : IQueryHandler<GetPendingPackagePaymentStatusQuery, PendingPackagePaymentStatusDto>
{
    private readonly IPendingPackagePaymentStatusStore _store;

    public GetPendingPackagePaymentStatusQueryHandler(IPendingPackagePaymentStatusStore store)
    {
        _store = store;
    }

    public Task<Result<PendingPackagePaymentStatusDto>> Handle(
        GetPendingPackagePaymentStatusQuery request,
        CancellationToken cancellationToken)
    {
        if (_store.TryGet(request.BookingId, out var status))
        {
            return Task.FromResult(Result.Success(status));
        }

        return Task.FromResult(Result.Failure<PendingPackagePaymentStatusDto>(
            new Error("Booking.PendingPaymentStatusNotReady", "Payment session is not ready yet.")));
    }
}
