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

public sealed class CreateBookingCommandHandler : ICommandHandler<CreateBookingCommand, BookingDto>
{
    private readonly IBookingRepository _bookingRepository;
    private readonly ISubscriptionRepository _subscriptionRepository;
    private readonly ITimeslotRepository _timeslotRepository;
    private readonly ISubscriptionEventRepository _subscriptionEventRepository;
    private readonly IMapper _mapper;

    public CreateBookingCommandHandler(
        IBookingRepository bookingRepository,
        ISubscriptionRepository subscriptionRepository,
        ITimeslotRepository timeslotRepository,
        ISubscriptionEventRepository subscriptionEventRepository,
        IMapper mapper)
    {
        _bookingRepository = bookingRepository;
        _subscriptionRepository = subscriptionRepository;
        _timeslotRepository = timeslotRepository;
        _subscriptionEventRepository = subscriptionEventRepository;
        _mapper = mapper;
    }

    public async Task<Result<BookingDto>> Handle(CreateBookingCommand request, CancellationToken cancellationToken)
    {
        var utcNow = DateTime.UtcNow;

        Timeslot? timeslot = null;
        var startAt = request.StartAt;
        var endAt = request.EndAt;

        if (request.TimeslotId.HasValue)
        {
            timeslot = await _timeslotRepository.FindByIdAsync(request.TimeslotId.Value, cancellationToken);
            if (timeslot is null)
            {
                return Result.Failure<BookingDto>(BookingErrors.TimeslotNotFound(request.TimeslotId.Value));
            }

            if (timeslot.CoachId != request.CoachId)
            {
                return Result.Failure<BookingDto>(BookingErrors.TimeslotCoachMismatch(timeslot.Id, request.CoachId));
            }

            startAt = timeslot.StartAt;
            endAt = timeslot.EndAt;
        }

        Subscription? subscription = null;
        if (timeslot is not null)
        {
            subscription = await _subscriptionRepository.FindActiveForBookingAsync(
                request.UserId,
                request.CoachId,
                startAt,
                endAt,
                cancellationToken);
        }

        if (subscription is not null)
        {
            if (subscription.SessionsReserved >= subscription.SessionsTotal)
            {
                return Result.Failure<BookingDto>(BookingErrors.SubscriptionSessionsExhausted(subscription.Id));
            }

            if (timeslot is null)
            {
                return Result.Failure<BookingDto>(BookingErrors.TimeslotRequired(request.UserId, request.CoachId));
            }

            if (timeslot.Status != SlotStatus.Open)
            {
                return Result.Failure<BookingDto>(BookingErrors.TimeslotNotOpen(timeslot.Id));
            }

            if (startAt < subscription.PeriodStart || endAt > subscription.PeriodEnd)
            {
                return Result.Failure<BookingDto>(BookingErrors.TimeslotOutsideSubscription(timeslot.Id, subscription.Id));
            }

            var durationMinutes = request.DurationMinutes ?? (int?)(endAt - startAt).TotalMinutes;

            var bookingWithSubscription = new Booking
            {
                Id = Guid.NewGuid(),
                UserId = request.UserId,
                CoachId = request.CoachId,
                TimeslotId = timeslot.Id,
                StartAt = startAt,
                EndAt = endAt,
                Status = BookingStatus.Confirmed,
                GrossAmountVnd = 0,
                CommissionPct = subscription.CommissionPct,
                CommissionVnd = 0,
                NetAmountVnd = 0,
                CurrencyCode = subscription.CurrencyCode,
                LocationNote = request.LocationNote,
                Notes = request.Notes,
                DurationMinutes = durationMinutes,
                CreatedAt = utcNow,
                UpdatedAt = utcNow,
                Timeslot = timeslot
            };

            await _bookingRepository.AddAsync(bookingWithSubscription, cancellationToken);

            subscription.SessionsReserved += 1;
            subscription.UpdatedAt = utcNow;
            _subscriptionRepository.Update(subscription);

            timeslot.Status = SlotStatus.Reserved;
            timeslot.UpdatedAt = utcNow;
            _timeslotRepository.Update(timeslot);

            var subscriptionEvent = new SubscriptionEvent
            {
                Id = Guid.NewGuid(),
                SubscriptionId = subscription.Id,
                BookingId = bookingWithSubscription.Id,
                TimeslotId = timeslot.Id,
                EventType = SubscriptionEventType.SlotBooked,
                CreatedAt = utcNow,
                Booking = bookingWithSubscription,
                Timeslot = timeslot,
                Subscription = subscription
            };

            await _subscriptionEventRepository.AddAsync(subscriptionEvent, cancellationToken);
            bookingWithSubscription.SubscriptionEvents.Add(subscriptionEvent);

            var persistedBooking = await _bookingRepository.GetDetailedByIdAsync(
                bookingWithSubscription.Id,
                cancellationToken,
                asNoTracking: true) ?? bookingWithSubscription;

            var bookingDto = _mapper.Map<BookingDto>(persistedBooking);
            return Result.Success(bookingDto);
        }

        var booking = new Booking
        {
            Id = Guid.NewGuid(),
            UserId = request.UserId,
            CoachId = request.CoachId,
            TimeslotId = request.TimeslotId,
            StartAt = startAt,
            EndAt = endAt,
            Status = request.Status,
            GrossAmountVnd = request.GrossAmountVnd,
            CommissionPct = request.CommissionPct,
            CommissionVnd = request.CommissionVnd,
            NetAmountVnd = request.NetAmountVnd,
            CurrencyCode = request.CurrencyCode,
            LocationNote = request.LocationNote,
            Notes = request.Notes,
            DurationMinutes = request.DurationMinutes ?? (timeslot is not null ? (int?)(endAt - startAt).TotalMinutes : null),
            CreatedAt = utcNow,
            UpdatedAt = utcNow,
            Timeslot = timeslot
        };

        await _bookingRepository.AddAsync(booking, cancellationToken);

        var persisted = await _bookingRepository.GetDetailedByIdAsync(booking.Id, cancellationToken, asNoTracking: true) ?? booking;
        var dto = _mapper.Map<BookingDto>(persisted);
        return Result.Success(dto);
    }
}
