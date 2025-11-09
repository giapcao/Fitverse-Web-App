using Application.Abstractions.Messaging;
using Application.Reviews.Command;
using Application.Reviews.Dtos;
using Domain.IRepositories;
using Domain.Persistence.Models;
using MapsterMapper;
using SharedLibrary.Common.ResponseModel;

namespace Application.Reviews.Handler;

public sealed class CreateReviewCommandHandler
    : ICommandHandler<CreateReviewCommand, ReviewDto>
{
    private readonly IReviewRepository _reviewRepository;
    private readonly IMapper _mapper;

    public CreateReviewCommandHandler(IReviewRepository reviewRepository, IMapper mapper)
    {
        _reviewRepository = reviewRepository;
        _mapper = mapper;
    }

    public async Task<Result<ReviewDto>> Handle(CreateReviewCommand request, CancellationToken cancellationToken)
    {
        var exists = await _reviewRepository.ExistsForBookingAsync(request.BookingId, cancellationToken);
        if (exists)
        {
            return Result.Failure<ReviewDto>(new Error("Review.Exists", "A review for this booking already exists."));
        }

        var review = new Review
        {
            Id = Guid.NewGuid(),
            BookingId = request.BookingId,
            UserId = request.UserId,
            CoachId = request.CoachId,
            Rating = request.Rating,
            Comment = request.Comment,
            IsPublic = request.IsPublic,
            CreatedAt = DateTime.UtcNow
        };

        await _reviewRepository.AddAsync(review, cancellationToken);

        return Result.Success(_mapper.Map<ReviewDto>(review));
    }
}

