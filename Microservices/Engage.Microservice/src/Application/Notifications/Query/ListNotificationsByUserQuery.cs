using Application.Abstractions.Messaging;
using Application.Notifications.Dtos;

namespace Application.Notifications.Query;

public sealed record ListNotificationsByUserQuery(Guid UserId, int? Take) : IQuery<IReadOnlyList<NotificationDto>>;

