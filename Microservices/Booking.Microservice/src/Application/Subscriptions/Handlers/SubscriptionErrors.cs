using System;
using SharedLibrary.Common.ResponseModel;

namespace Application.Subscriptions.Handlers;

internal static class SubscriptionErrors
{
    internal static Error NotFound(Guid id) => new("Subscription.NotFound", $"Subscription '{id}' was not found.");
}
