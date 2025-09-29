using Application.Abstractions.Interface;
using Application.Abstractions.Messaging;
using Application.Authentication.Command;
using MediatR;
using SharedLibrary.Common.ResponseModel;

namespace Application.Authentication.Handler;

public sealed class LogoutCommandHandler : ICommandHandler<LogoutCommand, Unit>
{
    private readonly ICurrentUser _current;
    private readonly IRefreshTokenStore _refresh;

    public LogoutCommandHandler(ICurrentUser current, IRefreshTokenStore refresh)
    {
        _current = current; _refresh = refresh;
    }

    public async Task<Result<Unit>> Handle(LogoutCommand request, CancellationToken ct)
    {
        if (!_current.IsAuthenticated || _current.UserId is null)
            throw new UnauthorizedAccessException();
        
        await _refresh.RevokeAsync(_current.UserId.Value,ct);
        return Result.Success(Unit.Value);
    }
}