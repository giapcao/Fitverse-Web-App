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
    public CreateSportCommandHandler(ISportRepository repository)
    {
        _repository = repository;
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

        var created = await _repository.GetByIdAsync(sport.Id, cancellationToken, asNoTracking: true) ?? sport;
        return Result.Success(SportMapping.ToDto(created));
    }
}

