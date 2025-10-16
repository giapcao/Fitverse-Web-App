using System;
using System.Threading;
using System.Threading.Tasks;
using Application.Abstractions.Messaging;
using Application.Bookings.Commands;
using Application.Features;
using Domain.IRepositories;
using Domain.Persistence.Models;
using MapsterMapper;
using SharedLibrary.Common;
using SharedLibrary.Common.ResponseModel;

namespace Application.Bookings.Handlers;

public sealed class CreateBookingCommandHandler : ICommandHandler<CreateBookingCommand, BookingDto>
{
    private readonly IBookingRepository _bookingRepository;
    private readonly IMapper _mapper;

    public CreateBookingCommandHandler(
        IBookingRepository bookingRepository,
        IMapper mapper)
    {
        _bookingRepository = bookingRepository;
        _mapper = mapper;
    }

    public async Task<Result<BookingDto>> Handle(CreateBookingCommand request, CancellationToken cancellationToken)
    {
        var utcNow = DateTime.UtcNow;
        var booking = new Booking
        {
            Id = Guid.NewGuid(),
            UserId = request.UserId,
            CoachId = request.CoachId,
            ServiceId = request.ServiceId,
            TimeslotId = request.TimeslotId,
            StartAt = request.StartAt,
            EndAt = request.EndAt,
            Status = request.Status,
            GrossAmountVnd = request.GrossAmountVnd,
            CommissionPct = request.CommissionPct,
            CommissionVnd = request.CommissionVnd,
            NetAmountVnd = request.NetAmountVnd,
            CurrencyCode = request.CurrencyCode,
            LocationNote = request.LocationNote,
            Notes = request.Notes,
            ServiceTitle = request.ServiceTitle,
            DurationMinutes = request.DurationMinutes,
            CreatedAt = utcNow,
            UpdatedAt = utcNow
        };

        await _bookingRepository.AddAsync(booking, cancellationToken);

        var persisted = await _bookingRepository.GetDetailedByIdAsync(booking.Id, cancellationToken, asNoTracking: true) ?? booking;
        var dto = _mapper.Map<BookingDto>(persisted);
        return Result.Success(dto);
    }
}
