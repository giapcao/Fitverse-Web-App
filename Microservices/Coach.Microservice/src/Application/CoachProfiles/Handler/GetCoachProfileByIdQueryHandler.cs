using System.Threading;
using System.Threading.Tasks;
using Application.Abstractions.Messaging;
using Application.CoachProfiles.Query;
using Application.Features;
using Domain.IRepositories;
using SharedLibrary.Common.ResponseModel;
using SharedLibrary.Storage;

namespace Application.CoachProfiles.Handler;

public sealed class GetCoachProfileByIdQueryHandler : IQueryHandler<GetCoachProfileByIdQuery, CoachProfileDto>
{
    private readonly ICoachProfileRepository _repository;
    private readonly IFileStorageService _fileStorageService;

    public GetCoachProfileByIdQueryHandler(ICoachProfileRepository repository, IFileStorageService fileStorageService)
    {
        _repository = repository;
        _fileStorageService = fileStorageService;
    }

    public async Task<Result<CoachProfileDto>> Handle(GetCoachProfileByIdQuery request, CancellationToken cancellationToken)
    {
        var profile = await _repository.GetDetailedByUserIdAsync(request.CoachId, cancellationToken, asNoTracking: true);
        if (profile is null)
        {
            return Result.Failure<CoachProfileDto>(new Error("CoachProfile.NotFound", $"Coach profile {request.CoachId} was not found."));
        }

        var dto = CoachProfileMapping.ToDto(profile);
        dto = await CoachProfileFileUrlHelper.WithSignedUrlsAsync(dto, _fileStorageService, cancellationToken);
        return Result.Success(dto);
    }
}

