using Application.Abstractions.Messaging;
using MediatR;

namespace Application.Authentication.Command;

public sealed record ResetPasswordWithTokenCommand(string ResetToken, string NewPassword)
    : ICommand<Unit>;