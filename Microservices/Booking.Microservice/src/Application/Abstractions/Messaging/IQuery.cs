using MediatR;
using SharedLibrary.Common.ResponseModel;

namespace Application.Abstractions.Messaging;

public interface IQuery<TResponse> : IRequest<Result<TResponse>>
{
}
