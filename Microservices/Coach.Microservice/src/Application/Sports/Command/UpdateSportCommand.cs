using System;
using Application.Abstractions.Messaging;
using Application.Features;

namespace Application.Sports.Command;

public sealed record UpdateSportCommand(
    Guid SportId,
    string? DisplayName,
    string? Description) : ICommand<SportDto>;
