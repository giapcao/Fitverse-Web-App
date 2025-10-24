using System;
using SharedLibrary.Common.ResponseModel;

namespace Application.AvailabilityRules.Handlers;

internal static class AvailabilityRuleErrors
{
    internal static Error NotFound(Guid id) =>
        new("AvailabilityRule.NotFound", $"Availability rule '{id}' was not found.");
}
