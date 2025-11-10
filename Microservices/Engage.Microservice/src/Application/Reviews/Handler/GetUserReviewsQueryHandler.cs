using System.Linq;
using Application.Abstractions.Messaging;
using Application.Reviews.Dtos;
using Application.Reviews.Query;
using Domain.IRepositories;
using MapsterMapper;
using SharedLibrary.Common.ResponseModel;

namespace Application.Reviews.Handler;

public sealed class GetUserReviewsQueryHandler
    : IQueryHandler<GetUserReviewsQuery, IReadOnlyList<ReviewDto>>
{
    private readonly IReviewRepository _reviewRepository;
    private readonly IMapper _mapper;

    public GetUserReviewsQueryHandler(IReviewRepository reviewRepository, IMapper mapper)
    {
        _reviewRepository = reviewRepository;
        _mapper = mapper;
    }

    public async Task<Result<IReadOnlyList<ReviewDto>>> Handle(GetUserReviewsQuery request, CancellationToken cancellationToken)
    {
        var reviews = await _reviewRepository.GetUserReviewsAsync(request.UserId, cancellationToken);
        var dtos = reviews
            .Select(review => _mapper.Map<ReviewDto>(review))
            .ToList()
            .AsReadOnly();

        return Result.Success<IReadOnlyList<ReviewDto>>(dtos);
    }
}

