using MediatR;
using SharedLibrary.Common.ResponseModel;

namespace Application.Abstractions.Messaging;

public interface ICommand : IRequest<Result>
{
}

public interface ICommand<TResponse> : IRequest<Result<TResponse>>
{
}
