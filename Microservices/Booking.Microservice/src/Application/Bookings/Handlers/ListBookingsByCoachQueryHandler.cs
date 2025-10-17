using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Application.Abstractions.Messaging;
using Application.Bookings.Queries;
using Application.Features;
using Domain.IRepositories;
using MapsterMapper;
using SharedLibrary.Common.ResponseModel;

namespace Application.Bookings.Handlers;

public sealed class ListBookingsByCoachQueryHandler : IQueryHandler<ListBookingsByCoachQuery, IReadOnlyCollection<BookingDto>>
{
    private readonly IBookingRepository _bookingRepository;
    private readonly IMapper _mapper;

    public ListBookingsByCoachQueryHandler(IBookingRepository bookingRepository, IMapper mapper)
    {
        _bookingRepository = bookingRepository;
        _mapper = mapper;
    }

    public async Task<Result<IReadOnlyCollection<BookingDto>>> Handle(ListBookingsByCoachQuery request, CancellationToken cancellationToken)
    {
        var bookings = await _bookingRepository.GetByCoachIdAsync(request.CoachId, cancellationToken, asNoTracking: true);
        var dto = bookings.Select(b => _mapper.Map<BookingDto>(b)).ToList();
        return Result.Success<IReadOnlyCollection<BookingDto>>(dto);
    }
}
