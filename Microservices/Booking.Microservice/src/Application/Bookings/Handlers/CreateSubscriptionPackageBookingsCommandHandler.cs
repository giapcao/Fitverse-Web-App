using System;
using System.Collections.Generic;
using System.Linq;
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

public sealed class CreateSubscriptionPackageBookingsCommandHandler
    : ICommandHandler<CreateSubscriptionPackageBookingsCommand, IReadOnlyCollection<BookingDto>>
{
    private readonly ITimeslotRepository _timeslotRepository;
    private readonly ISubscriptionRepository _subscriptionRepository;
    private readonly ISubscriptionEventRepository _subscriptionEventRepository;
    private readonly IBookingRepository _bookingRepository;
    private readonly IMapper _mapper;

    public CreateSubscriptionPackageBookingsCommandHandler(
        ITimeslotRepository timeslotRepository,
        ISubscriptionRepository subscriptionRepository,
        ISubscriptionEventRepository subscriptionEventRepository,
        IBookingRepository bookingRepository,
        IMapper mapper)
    {
        _timeslotRepository = timeslotRepository;
        _subscriptionRepository = subscriptionRepository;
        _subscriptionEventRepository = subscriptionEventRepository;
        _bookingRepository = bookingRepository;
        _mapper = mapper;
    }

    public async Task<Result<IReadOnlyCollection<BookingDto>>> Handle(
        CreateSubscriptionPackageBookingsCommand request,
        CancellationToken cancellationToken)
    {
        if (request.TimeslotIds.Count == 0)
        {
            return Result.Failure<IReadOnlyCollection<BookingDto>>(BookingErrors.NoTimeslotsProvided());
        }

        var distinctTimeslotIds = request.TimeslotIds.Distinct().ToList();
        var timeslots = new List<Timeslot>(distinctTimeslotIds.Count);

        foreach (var timeslotId in distinctTimeslotIds)
        {
            var timeslot = await _timeslotRepository.FindByIdAsync(timeslotId, cancellationToken);
            if (timeslot is null)
            {
                return Result.Failure<IReadOnlyCollection<BookingDto>>(BookingErrors.TimeslotNotFound(timeslotId));
            }

            if (timeslot.CoachId != request.CoachId)
            {
                return Result.Failure<IReadOnlyCollection<BookingDto>>(BookingErrors.TimeslotCoachMismatch(timeslot.Id, request.CoachId));
            }

            if (timeslot.Status != SlotStatus.Open)
            {
                return Result.Failure<IReadOnlyCollection<BookingDto>>(BookingErrors.TimeslotNotOpen(timeslot.Id));
            }

            timeslots.Add(timeslot);
        }

        timeslots.Sort((left, right) => left.StartAt.CompareTo(right.StartAt));

        var earliestStart = timeslots.First().StartAt;
        var latestEnd = timeslots.Last().EndAt;

        var subscription = await _subscriptionRepository.FindActiveForBookingAsync(
            request.UserId,
            request.CoachId,
            earliestStart,
            latestEnd,
            cancellationToken);

        if (subscription is null)
        {
            return Result.Failure<IReadOnlyCollection<BookingDto>>(BookingErrors.SubscriptionUnavailable(request.UserId, request.CoachId));
        }

        var hasLockedPeriod = subscription.PeriodEnd > subscription.PeriodStart;

        foreach (var timeslot in timeslots)
        {
            if (timeslot.StartAt < subscription.PeriodStart || (hasLockedPeriod && timeslot.EndAt > subscription.PeriodEnd))
            {
                return Result.Failure<IReadOnlyCollection<BookingDto>>(BookingErrors.TimeslotOutsideSubscription(timeslot.Id, subscription.Id));
            }
        }

        var timeslotCount = timeslots.Count;
        var remainingSessions = subscription.SessionsTotal - subscription.SessionsReserved;
        if (remainingSessions < timeslotCount)
        {
            return Result.Failure<IReadOnlyCollection<BookingDto>>(
                BookingErrors.SubscriptionInsufficientSessions(subscription.Id, timeslotCount, remainingSessions));
        }

        var utcNow = DateTime.UtcNow;
        var bookings = new List<Booking>(timeslotCount);

        foreach (var timeslot in timeslots)
        {
            timeslot.Status = SlotStatus.Reserved;
            timeslot.UpdatedAt = utcNow;
            _timeslotRepository.Update(timeslot);

            var booking = new Booking
            {
                Id = Guid.NewGuid(),
                UserId = request.UserId,
                CoachId = request.CoachId,
                TimeslotId = timeslot.Id,
                StartAt = timeslot.StartAt,
                EndAt = timeslot.EndAt,
                Status = BookingStatus.Paid,
                LocationNote = request.LocationNote,
                Notes = request.Notes,
                DurationMinutes = (int)(timeslot.EndAt - timeslot.StartAt).TotalMinutes,
                CreatedAt = utcNow,
                UpdatedAt = utcNow,
                Timeslot = timeslot
            };

            await _bookingRepository.AddAsync(booking, cancellationToken);

            var subscriptionEvent = new SubscriptionEvent
            {
                Id = Guid.NewGuid(),
                SubscriptionId = subscription.Id,
                BookingId = booking.Id,
                TimeslotId = timeslot.Id,
                EventType = SubscriptionEventType.SlotBooked,
                CreatedAt = utcNow,
                Booking = booking,
                Timeslot = timeslot,
                Subscription = subscription
            };

            await _subscriptionEventRepository.AddAsync(subscriptionEvent, cancellationToken);
            booking.SubscriptionEvents.Add(subscriptionEvent);
            bookings.Add(booking);
        }

        subscription.SessionsReserved += timeslotCount;
        if (subscription.SessionsReserved >= subscription.SessionsTotal)
        {
            subscription.PeriodEnd = latestEnd;
        }
        subscription.UpdatedAt = utcNow;
        _subscriptionRepository.Update(subscription);

        var bookingDtos = new List<BookingDto>(bookings.Count);
        foreach (var booking in bookings)
        {
            var persistedBooking = await _bookingRepository.GetDetailedByIdAsync(
                booking.Id,
                cancellationToken,
                asNoTracking: true) ?? booking;

            bookingDtos.Add(_mapper.Map<BookingDto>(persistedBooking));
        }

        return Result.Success<IReadOnlyCollection<BookingDto>>(bookingDtos);
    }
}
