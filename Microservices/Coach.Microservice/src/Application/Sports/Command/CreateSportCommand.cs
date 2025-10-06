using Application.Abstractions.Messaging;
using Application.Features;

namespace Application.Sports.Command;

public sealed record CreateSportCommand(
    string DisplayName,
    string? Description) : ICommand<SportDto>;
