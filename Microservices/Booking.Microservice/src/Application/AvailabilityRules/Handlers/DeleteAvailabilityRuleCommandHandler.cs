using System.Threading;
using System.Threading.Tasks;
using Application.Abstractions.Messaging;
using Application.AvailabilityRules.Commands;
using Domain.IRepositories;
using SharedLibrary.Common;
using SharedLibrary.Common.ResponseModel;

namespace Application.AvailabilityRules.Handlers;

public sealed class DeleteAvailabilityRuleCommandHandler : ICommandHandler<DeleteAvailabilityRuleCommand>
{
    private readonly IAvailabilityRuleRepository _availabilityRuleRepository;

    public DeleteAvailabilityRuleCommandHandler(
        IAvailabilityRuleRepository availabilityRuleRepository)
    {
        _availabilityRuleRepository = availabilityRuleRepository;
    }

    public async Task<Result> Handle(DeleteAvailabilityRuleCommand request, CancellationToken cancellationToken)
    {
        var rule = await _availabilityRuleRepository.FindByIdAsync(request.Id, cancellationToken);
        if (rule is null)
        {
            return Result.Failure(AvailabilityRuleErrors.NotFound(request.Id));
        }

        _availabilityRuleRepository.Delete(rule);

        return Result.Success();
    }
}
