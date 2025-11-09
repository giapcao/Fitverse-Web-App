using Application.Abstractions.Messaging;
using Application.Reviews.Dtos;
using Application.Reviews.Query;
using Domain.IRepositories;
using MapsterMapper;
using SharedLibrary.Common.ResponseModel;

namespace Application.Reviews.Handler;

public sealed class GetReviewByBookingIdQueryHandler
    : IQueryHandler<GetReviewByBookingIdQuery, ReviewDto>
{
    private readonly IReviewRepository _reviewRepository;
    private readonly IMapper _mapper;

    public GetReviewByBookingIdQueryHandler(IReviewRepository reviewRepository, IMapper mapper)
    {
        _reviewRepository = reviewRepository;
        _mapper = mapper;
    }

    public async Task<Result<ReviewDto>> Handle(GetReviewByBookingIdQuery request, CancellationToken cancellationToken)
    {
        var review = await _reviewRepository.GetByBookingIdAsync(request.BookingId, cancellationToken);
        if (review is null)
        {
            return Result.Failure<ReviewDto>(new Error("Review.NotFound", "Review not found."));
        }

        return Result.Success(_mapper.Map<ReviewDto>(review));
    }
}

