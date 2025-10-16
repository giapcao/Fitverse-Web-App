using System;
using SharedLibrary.Common.ResponseModel;

namespace Application.CoachTimeoffs.Handlers;

internal static class CoachTimeoffErrors
{
    internal static Error NotFound(Guid id) => new("CoachTimeoff.NotFound", $"Coach timeoff '{id}' was not found.");
}
