using System;
using System.Threading;
using System.Threading.Tasks;
using Application.Abstractions.Messaging;
using Application.Bookings.Commands;
using Application.Features;
using Domain.IRepositories;
using Domain.Persistence.Enums;
using MapsterMapper;
using SharedLibrary.Common;
using SharedLibrary.Common.ResponseModel;

namespace Application.Bookings.Handlers;

public sealed class UpdateBookingStatusCommandHandler
    : ICommandHandler<UpdateBookingStatusCommand, BookingDto>
{
    private readonly IBookingRepository _bookingRepository;
    private readonly IMapper _mapper;

    public UpdateBookingStatusCommandHandler(
        IBookingRepository bookingRepository,
        IMapper mapper)
    {
        _bookingRepository = bookingRepository;
        _mapper = mapper;
    }

    public async Task<Result<BookingDto>> Handle(UpdateBookingStatusCommand request, CancellationToken cancellationToken)
    {
        if (request.RequestedStatus is not (BookingStatus.ConfirmedByCoach or BookingStatus.ConfirmedByUser))
        {
            return Result.Failure<BookingDto>(BookingErrors.InvalidRequestedConfirmationStatus(request.RequestedStatus));
        }

        var booking = await _bookingRepository.FindByIdAsync(request.BookingId, cancellationToken);
        if (booking is null)
        {
            return Result.Failure<BookingDto>(BookingErrors.NotFound(request.BookingId));
        }

        if (booking.Status is not (BookingStatus.Paid or BookingStatus.ConfirmedByCoach or BookingStatus.ConfirmedByUser))
        {
            return Result.Failure<BookingDto>(BookingErrors.InvalidStatusForConfirmation(booking.Id, booking.Status));
        }

        var nextStatus = booking.Status == BookingStatus.Paid
            ? request.RequestedStatus
            : BookingStatus.Confirmed;

        booking.Status = nextStatus;
        booking.UpdatedAt = DateTime.UtcNow;

        _bookingRepository.Update(booking);

        var persisted = await _bookingRepository.GetDetailedByIdAsync(booking.Id, cancellationToken, asNoTracking: true) ?? booking;
        var dto = _mapper.Map<BookingDto>(persisted);
        return Result.Success(dto);
    }
}
