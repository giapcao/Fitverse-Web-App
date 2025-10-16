using System;
using Application.Abstractions.Messaging;

namespace Application.Bookings.Commands;

public sealed record DeleteBookingCommand(Guid Id) : ICommand;
