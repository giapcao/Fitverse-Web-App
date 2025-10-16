using System;
using Application.Abstractions.Messaging;

namespace Application.AvailabilityRules.Commands;

public sealed record DeleteAvailabilityRuleCommand(Guid Id) : ICommand;
