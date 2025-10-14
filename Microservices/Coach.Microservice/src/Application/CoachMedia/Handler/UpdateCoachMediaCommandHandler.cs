using System;
using System.Threading;
using System.Threading.Tasks;
using Application.Abstractions.Messaging;
using Application.CoachMedia.Command;
using Application.Features;
using Domain.IRepositories;
using SharedLibrary.Common;
using SharedLibrary.Common.ResponseModel;

namespace Application.CoachMedia.Handler;

public sealed class UpdateCoachMediaCommandHandler : ICommandHandler<UpdateCoachMediaCommand, CoachMediaDto>
{
    private readonly ICoachMediaRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateCoachMediaCommandHandler(ICoachMediaRepository repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<CoachMediaDto>> Handle(UpdateCoachMediaCommand request, CancellationToken cancellationToken)
    {
        var medium = await _repository.GetDetailedByIdAsync(request.MediaId, cancellationToken);
        if (medium is null)
        {
            return Result.Failure<CoachMediaDto>(new Error("CoachMedia.NotFound", $"Coach media {request.MediaId} was not found."));
        }

        medium.MediaName = request.MediaName ?? medium.MediaName;
        if (request.Description is not null)
        {
            medium.Description = request.Description;
        }

        if (request.MediaType.HasValue)
        {
            medium.MediaType = request.MediaType.Value;
        }

        if (!string.IsNullOrWhiteSpace(request.Url))
        {
            medium.Url = request.Url!;
        }

        if (request.Status.HasValue)
        {
            medium.Status = request.Status.Value;
        }

        if (request.IsFeatured.HasValue)
        {
            medium.IsFeatured = request.IsFeatured.Value;
        }

        medium.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var updated = await _repository.GetDetailedByIdAsync(medium.Id, cancellationToken, asNoTracking: true) ?? medium;
        return Result.Success(CoachMediaMapping.ToDto(updated));
    }
}
