using System.Threading;
using System.Threading.Tasks;
using Application.Abstractions.Messaging;
using Application.CoachMedia.Query;
using Application.Features;
using Domain.IRepositories;
using SharedLibrary.Common.ResponseModel;
using SharedLibrary.Storage;

namespace Application.CoachMedia.Handler;

public sealed class GetCoachMediaByIdQueryHandler : IQueryHandler<GetCoachMediaByIdQuery, CoachMediaDto>
{
    private readonly ICoachMediaRepository _repository;
    private readonly IFileStorageService _fileStorageService;

    public GetCoachMediaByIdQueryHandler(ICoachMediaRepository repository, IFileStorageService fileStorageService)
    {
        _repository = repository;
        _fileStorageService = fileStorageService;
    }

    public async Task<Result<CoachMediaDto>> Handle(GetCoachMediaByIdQuery request, CancellationToken cancellationToken)
    {
        var medium = await _repository.GetDetailedByIdAsync(request.MediaId, cancellationToken, asNoTracking: true);
        if (medium is null)
        {
            return Result.Failure<CoachMediaDto>(new Error("CoachMedia.NotFound", $"Coach media {request.MediaId} was not found."));
        }

        var dto = CoachMediaMapping.ToDto(medium);
        dto = await CoachMediaFileUrlHelper.WithSignedFileUrlAsync(dto, _fileStorageService, cancellationToken);
        return Result.Success(dto);
    }
}
