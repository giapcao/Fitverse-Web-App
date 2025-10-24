using System;
using Application.Abstractions.Messaging;

namespace Application.Timeslots.Commands;

public sealed record DeleteTimeslotCommand(Guid Id) : ICommand;
