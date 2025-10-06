using System;
using System.Threading;
using System.Threading.Tasks;
using Application.Abstractions.Messaging;
using Application.Features;
using Application.Sports.Command;
using Domain.IRepositories;
using Domain.Persistence.Models;
using SharedLibrary.Common;
using SharedLibrary.Common.ResponseModel;

namespace Application.Sports.Handler;

public sealed class CreateSportCommandHandler : ICommandHandler<CreateSportCommand, SportDto>
{
    private readonly ISportRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateSportCommandHandler(ISportRepository repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<SportDto>> Handle(CreateSportCommand request, CancellationToken cancellationToken)
    {
        var sport = new Sport
        {
            Id = Guid.NewGuid(),
            DisplayName = request.DisplayName,
            Description = request.Description
        };

        await _repository.AddAsync(sport, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var created = await _repository.GetByIdAsync(sport.Id, cancellationToken, asNoTracking: true) ?? sport;
        return Result.Success(SportMapping.ToDto(created));
    }
}
