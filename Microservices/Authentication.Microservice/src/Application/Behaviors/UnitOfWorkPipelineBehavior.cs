using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Application.Abstractions.Messaging;
using MediatR;
using Microsoft.Extensions.Logging;
using SharedLibrary.Common;
using SharedLibrary.Common.ResponseModel;

namespace Application.Behaviors;

public sealed class UnitOfWorkPipelineBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private static readonly bool IsCommandRequest = typeof(ICommand).IsAssignableFrom(typeof(TRequest)) ||
        typeof(TRequest).GetInterfaces().Any(interfaceType =>
            interfaceType == typeof(ICommand) ||
            interfaceType.IsGenericType && interfaceType.GetGenericTypeDefinition() == typeof(ICommand<>));

    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<UnitOfWorkPipelineBehavior<TRequest, TResponse>> _logger;

    public UnitOfWorkPipelineBehavior(
        IUnitOfWork unitOfWork,
        ILogger<UnitOfWorkPipelineBehavior<TRequest, TResponse>> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        if (!IsCommandRequest)
        {
            return await next();
        }

        TResponse response;
        try
        {
            response = await next();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Command {Command} threw an exception. Aborting persistence.", typeof(TRequest).Name);
            throw;
        }

        if (response is not Result result || result.IsFailure)
        {
            _logger.LogDebug("Command {Command} did not succeed; skipping persistence.", typeof(TRequest).Name);
            return response;
        }

        try
        {
            var affected = await _unitOfWork.SaveChangesAsync(cancellationToken);
            _logger.LogDebug("Command {Command} persisted {AffectedRows} changes.", typeof(TRequest).Name, affected);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Command {Command} failed during persistence.", typeof(TRequest).Name);
            throw;
        }

        return response;
    }
}

