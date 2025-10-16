using System;
using SharedLibrary.Common.ResponseModel;

namespace Application.Timeslots.Handlers;

internal static class TimeslotErrors
{
    internal static Error NotFound(Guid id) => new("Timeslot.NotFound", $"Timeslot '{id}' was not found.");
}
