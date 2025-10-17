using System;
using Application.Abstractions.Messaging;

namespace Application.SubscriptionEvents.Commands;

public sealed record DeleteSubscriptionEventCommand(Guid Id) : ICommand;
