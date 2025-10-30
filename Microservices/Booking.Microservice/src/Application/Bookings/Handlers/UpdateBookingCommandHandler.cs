using System;
using System.Threading;
using System.Threading.Tasks;
using Application.Abstractions.Messaging;
using Application.Bookings.Commands;
using Application.Features;
using Domain.IRepositories;
using MapsterMapper;
using SharedLibrary.Common;
using SharedLibrary.Common.ResponseModel;

namespace Application.Bookings.Handlers;

public sealed class UpdateBookingCommandHandler : ICommandHandler<UpdateBookingCommand, BookingDto>
{
    private readonly IBookingRepository _bookingRepository;
    private readonly IMapper _mapper;

    public UpdateBookingCommandHandler(
        IBookingRepository bookingRepository,
        IMapper mapper)
    {
        _bookingRepository = bookingRepository;
        _mapper = mapper;
    }

    public async Task<Result<BookingDto>> Handle(UpdateBookingCommand request, CancellationToken cancellationToken)
    {
        var booking = await _bookingRepository.FindByIdAsync(request.Id, cancellationToken);
        if (booking is null)
        {
            return Result.Failure<BookingDto>(BookingErrors.NotFound(request.Id));
        }

        booking.UserId = request.UserId;
        booking.CoachId = request.CoachId;
        booking.TimeslotId = request.TimeslotId;
        booking.StartAt = request.StartAt;
        booking.EndAt = request.EndAt;
        booking.Status = request.Status;
        booking.LocationNote = request.LocationNote;
        booking.Notes = request.Notes;
        booking.DurationMinutes = request.DurationMinutes;
        booking.UpdatedAt = DateTime.UtcNow;

        _bookingRepository.Update(booking);

        var persisted = await _bookingRepository.GetDetailedByIdAsync(booking.Id, cancellationToken, asNoTracking: true) ?? booking;
        var dto = _mapper.Map<BookingDto>(persisted);
        return Result.Success(dto);
    }
}
