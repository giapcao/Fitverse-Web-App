using System.Threading;
using System.Threading.Tasks;
using Application.Abstractions.Messaging;
using Application.Bookings.Queries;
using Application.Features;
using Domain.IRepositories;
using MapsterMapper;
using SharedLibrary.Common.ResponseModel;

namespace Application.Bookings.Handlers;

public sealed class GetBookingByIdQueryHandler : IQueryHandler<GetBookingByIdQuery, BookingDto>
{
    private readonly IBookingRepository _bookingRepository;
    private readonly IMapper _mapper;

    public GetBookingByIdQueryHandler(IBookingRepository bookingRepository, IMapper mapper)
    {
        _bookingRepository = bookingRepository;
        _mapper = mapper;
    }

    public async Task<Result<BookingDto>> Handle(GetBookingByIdQuery request, CancellationToken cancellationToken)
    {
        var booking = await _bookingRepository.GetDetailedByIdAsync(request.Id, cancellationToken, asNoTracking: true)
                      ?? await _bookingRepository.FindByIdAsync(request.Id, cancellationToken, asNoTracking: true);
        if (booking is null)
        {
            return Result.Failure<BookingDto>(BookingErrors.NotFound(request.Id));
        }

        var dto = _mapper.Map<BookingDto>(booking);
        return Result.Success(dto);
    }
}
