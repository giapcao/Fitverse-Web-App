using System;
using System.Threading;
using System.Threading.Tasks;
using Application.Abstractions.Messaging;
using Application.Bookings.Commands;
using Application.Features;
using Domain.IRepositories;
using Domain.Persistence.Enums;
using Domain.Persistence.Models;
using MapsterMapper;
using SharedLibrary.Common;
using SharedLibrary.Common.ResponseModel;

namespace Application.Bookings.Handlers;

public sealed class CancelPendingSubscriptionBookingCommandHandler
    : ICommandHandler<CancelPendingSubscriptionBookingCommand, BookingDto>
{
    private readonly ISubscriptionRepository _subscriptionRepository;
    private readonly IBookingRepository _bookingRepository;
    private readonly ITimeslotRepository _timeslotRepository;
    private readonly ISubscriptionEventRepository _subscriptionEventRepository;
    private readonly IMapper _mapper;

    public CancelPendingSubscriptionBookingCommandHandler(
        ISubscriptionRepository subscriptionRepository,
        IBookingRepository bookingRepository,
        ITimeslotRepository timeslotRepository,
        ISubscriptionEventRepository subscriptionEventRepository,
        IMapper mapper)
    {
        _subscriptionRepository = subscriptionRepository;
        _bookingRepository = bookingRepository;
        _timeslotRepository = timeslotRepository;
        _subscriptionEventRepository = subscriptionEventRepository;
        _mapper = mapper;
    }

    public async Task<Result<BookingDto>> Handle(
        CancelPendingSubscriptionBookingCommand request,
        CancellationToken cancellationToken)
    {
        var subscription = await _subscriptionRepository.FindByIdAsync(
            request.SubscriptionId,
            cancellationToken);

        if (subscription is null)
        {
            return Result.Failure<BookingDto>(BookingErrors.SubscriptionNotFound(request.SubscriptionId));
        }

        if (subscription.Status != SubscriptionStatus.Pending)
        {
            return Result.Failure<BookingDto>(BookingErrors.SubscriptionNotPending(subscription.Id));
        }

        var booking = await _bookingRepository.FindByIdAsync(
            request.BookingId,
            cancellationToken);

        if (booking is null)
        {
            return Result.Failure<BookingDto>(BookingErrors.NotFound(request.BookingId));
        }

        if (booking.Status != BookingStatus.PendingPayment)
        {
            return Result.Failure<BookingDto>(BookingErrors.BookingNotPendingPayment(booking.Id));
        }

        if (booking.TimeslotId is null)
        {
            return Result.Failure<BookingDto>(BookingErrors.BookingTimeslotMissing(booking.Id));
        }

        if (booking.UserId != subscription.UserId || booking.CoachId != subscription.CoachId)
        {
            return Result.Failure<BookingDto>(BookingErrors.BookingSubscriptionMismatch(booking.Id, subscription.Id));
        }

        var timeslot = await _timeslotRepository.FindByIdAsync(
            booking.TimeslotId.Value,
            cancellationToken);

        if (timeslot is null)
        {
            return Result.Failure<BookingDto>(BookingErrors.TimeslotNotFound(booking.TimeslotId.Value));
        }

        var utcNow = DateTime.UtcNow;

        subscription.Status = SubscriptionStatus.Canceled;
        subscription.UpdatedAt = utcNow;
        _subscriptionRepository.Update(subscription);

        booking.Status = BookingStatus.CancelledByUser;
        booking.UpdatedAt = utcNow;
        booking.Timeslot = timeslot;
        _bookingRepository.Update(booking);

        timeslot.Status = SlotStatus.Open;
        timeslot.UpdatedAt = utcNow;
        _timeslotRepository.Update(timeslot);

        var subscriptionEvent = new SubscriptionEvent
        {
            Id = Guid.NewGuid(),
            SubscriptionId = subscription.Id,
            BookingId = booking.Id,
            TimeslotId = timeslot.Id,
            EventType = SubscriptionEventType.SlotCancelled,
            CreatedAt = utcNow,
            Booking = booking,
            Timeslot = timeslot,
            Subscription = subscription
        };

        await _subscriptionEventRepository.AddAsync(subscriptionEvent, cancellationToken);
        booking.SubscriptionEvents.Add(subscriptionEvent);

        var persistedBooking = await _bookingRepository.GetDetailedByIdAsync(
            booking.Id,
            cancellationToken,
            asNoTracking: true) ?? booking;

        var dto = _mapper.Map<BookingDto>(persistedBooking);
        return Result.Success(dto);
    }
}
