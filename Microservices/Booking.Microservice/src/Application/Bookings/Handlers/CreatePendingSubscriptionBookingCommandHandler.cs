using Application.Abstractions.Messaging;
using Application.Bookings.Commands;
using Application.Features;
using Domain.IRepositories;
using Domain.Persistence.Enums;
using Domain.Persistence.Models;
using MapsterMapper;
using MassTransit;
using SharedLibrary.Common.ResponseModel;
using SharedLibrary.Contracts.Bookings;

namespace Application.Bookings.Handlers;

public sealed class CreatePendingSubscriptionBookingCommandHandler
    : ICommandHandler<CreatePendingSubscriptionBookingCommand, BookingDto>
{
    private readonly ITimeslotRepository _timeslotRepository;
    private readonly ISubscriptionRepository _subscriptionRepository;
    private readonly IBookingRepository _bookingRepository;
    private readonly IMapper _mapper;
    private readonly IPublishEndpoint _publishEndpoint;

    public CreatePendingSubscriptionBookingCommandHandler(
        ITimeslotRepository timeslotRepository,
        ISubscriptionRepository subscriptionRepository,
        IBookingRepository bookingRepository,
        IMapper mapper,
        IPublishEndpoint publishEndpoint)
    {
        _timeslotRepository = timeslotRepository;
        _subscriptionRepository = subscriptionRepository;
        _bookingRepository = bookingRepository;
        _mapper = mapper;
        _publishEndpoint = publishEndpoint;
    }

    public async Task<Result<BookingDto>> Handle(
        CreatePendingSubscriptionBookingCommand request,
        CancellationToken cancellationToken)
    {
        var timeslot = await _timeslotRepository.FindByIdAsync(
            request.TimeslotId,
            cancellationToken);

        if (timeslot is null)
        {
            return Result.Failure<BookingDto>(BookingErrors.TimeslotNotFound(request.TimeslotId));
        }

        if (timeslot.Status != SlotStatus.Open)
        {
            return Result.Failure<BookingDto>(BookingErrors.TimeslotNotOpen(timeslot.Id));
        }

        var utcNow = DateTime.UtcNow;

        timeslot.Status = SlotStatus.Pending;
        timeslot.UpdatedAt = utcNow;
        _timeslotRepository.Update(timeslot);

        var subscriptionPeriodStart = utcNow;
        var subscriptionPeriodEnd = request.SubscriptionSessionsTotal == 1
            ? timeslot.EndAt
            : subscriptionPeriodStart;

        var netAmountVnd = request.SubscriptionNetAmountVnd;
        var commissionVndDecimal = Math.Round(
            netAmountVnd * request.SubscriptionCommissionPct / 100m,
            MidpointRounding.AwayFromZero);
        var subscriptionCommissionVnd = Convert.ToInt64(commissionVndDecimal);
        var subscriptionPriceGrossVnd = checked(netAmountVnd + subscriptionCommissionVnd);

        var subscription = new Subscription
        {
            Id = Guid.NewGuid(),
            UserId = request.UserId,
            CoachId = timeslot.CoachId,
            ServiceId = request.ServiceId,
            Status = SubscriptionStatus.Pending,
            PeriodStart = subscriptionPeriodStart,
            PeriodEnd = subscriptionPeriodEnd,
            SessionsTotal = request.SubscriptionSessionsTotal,
            SessionsReserved = 0,
            SessionsConsumed = 0,
            PriceGrossVnd = subscriptionPriceGrossVnd,
            CommissionPct = request.SubscriptionCommissionPct,
            CommissionVnd = subscriptionCommissionVnd,
            NetAmountVnd = netAmountVnd,
            CurrencyCode = "VND",
            CreatedAt = utcNow,
            UpdatedAt = utcNow
        };

        await _subscriptionRepository.AddAsync(subscription, cancellationToken);

        var booking = new Booking
        {
            Id = Guid.NewGuid(),
            UserId = request.UserId,
            CoachId = timeslot.CoachId,
            TimeslotId = timeslot.Id,
            StartAt = timeslot.StartAt,
            EndAt = timeslot.EndAt,
            Status = BookingStatus.PendingPayment,
            LocationNote = request.BookingLocationNote,
            Notes = request.BookingNotes,
            DurationMinutes = request.BookingDurationMinutes
                ?? (int?)(timeslot.EndAt - timeslot.StartAt).TotalMinutes,
            CreatedAt = utcNow,
            UpdatedAt = utcNow,
            Timeslot = timeslot
        };

        await _bookingRepository.AddAsync(booking, cancellationToken);

        var persisted = await _bookingRepository.GetDetailedByIdAsync(
            booking.Id,
            cancellationToken,
            asNoTracking: true) ?? booking;

        var correlationId = request.CorrelationId ?? Guid.NewGuid();
        await _publishEndpoint.Publish(new PendingSubscriptionPackageSagaStart
        {
            CorrelationId = correlationId,
            SubscriptionId = subscription.Id,
            BookingId = booking.Id,
            UserId = request.UserId,
            CoachId = timeslot.CoachId,
            ServiceId = request.ServiceId,
            RequestedBookingId = booking.Id,
            WalletId = request.WalletId,
            TimeslotId = timeslot.Id,
            AmountVnd = subscriptionPriceGrossVnd,
            Gateway = request.Gateway,
            Flow = request.Flow,
            StartedAtUtc = utcNow,
            ClientIp = string.IsNullOrWhiteSpace(request.ClientIp) ? "127.0.0.1" : request.ClientIp
        }, cancellationToken);

        var dto = _mapper.Map<BookingDto>(persisted);
        return Result.Success(dto);
    }
}
