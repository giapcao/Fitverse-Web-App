using System;
using Application.Abstractions.Messaging;

namespace Application.Subscriptions.Commands;

public sealed record DeleteSubscriptionCommand(Guid Id) : ICommand;
