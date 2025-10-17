using System;
using SharedLibrary.Common.ResponseModel;

namespace Application.SubscriptionEvents.Handlers;

internal static class SubscriptionEventErrors
{
    internal static Error NotFound(Guid id) => new("SubscriptionEvent.NotFound", $"Subscription event '{id}' was not found.");
}
