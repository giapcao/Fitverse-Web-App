using System.Threading;
using System.Threading.Tasks;
using Application.Abstractions.Messaging;
using Application.Bookings.Commands;
using Domain.IRepositories;
using SharedLibrary.Common;
using SharedLibrary.Common.ResponseModel;

namespace Application.Bookings.Handlers;

public sealed class DeleteBookingCommandHandler : ICommandHandler<DeleteBookingCommand>
{
    private readonly IBookingRepository _bookingRepository;

    public DeleteBookingCommandHandler(IBookingRepository bookingRepository)
    {
        _bookingRepository = bookingRepository;
    }

    public async Task<Result> Handle(DeleteBookingCommand request, CancellationToken cancellationToken)
    {
        var booking = await _bookingRepository.FindByIdAsync(request.Id, cancellationToken);
        if (booking is null)
        {
            return Result.Failure(BookingErrors.NotFound(request.Id));
        }

        _bookingRepository.Delete(booking);

        return Result.Success();
    }
}
