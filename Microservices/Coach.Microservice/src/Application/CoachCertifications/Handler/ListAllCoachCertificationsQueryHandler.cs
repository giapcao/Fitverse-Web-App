using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Application.Abstractions.Messaging;
using Application.CoachCertifications.Query;
using Application.Features;
using Domain.IRepositories;
using SharedLibrary.Common.ResponseModel;
using SharedLibrary.Storage;

namespace Application.CoachCertifications.Handler;

public sealed class ListAllCoachCertificationsQueryHandler : IQueryHandler<ListAllCoachCertificationsQuery, PagedResult<CoachCertificationDto>>
{
    private readonly ICoachCertificationRepository _repository;
    private readonly IFileStorageService _fileStorageService;

    public ListAllCoachCertificationsQueryHandler(
        ICoachCertificationRepository repository,
        IFileStorageService fileStorageService)
    {
        _repository = repository;
        _fileStorageService = fileStorageService;
    }

    public async Task<Result<PagedResult<CoachCertificationDto>>> Handle(
        ListAllCoachCertificationsQuery request,
        CancellationToken cancellationToken)
    {
        var certifications = await _repository.GetAllAsync(cancellationToken);
        var dtoList = certifications.Select(CoachCertificationMapping.ToDto).ToList();
        var signedList = await CoachCertificationFileUrlHelper.WithSignedFileUrlsAsync(dtoList, _fileStorageService, cancellationToken);
        var pagedResult = PagedResult<CoachCertificationDto>.Create(signedList, request.PageNumber, request.PageSize);
        if (pagedResult.IsFailure)
        {
            return Result.Failure<PagedResult<CoachCertificationDto>>(pagedResult.Error);
        }

        return Result.Success(pagedResult);
    }
}
