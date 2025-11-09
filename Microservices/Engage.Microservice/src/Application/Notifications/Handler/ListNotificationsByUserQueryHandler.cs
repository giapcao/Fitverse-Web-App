using Application.Abstractions.Messaging;
using Application.Notifications.Dtos;
using Application.Notifications.Query;
using Domain.IRepositories;
using MapsterMapper;
using SharedLibrary.Common.ResponseModel;

namespace Application.Notifications.Handler;

public sealed class ListNotificationsByUserQueryHandler
    : IQueryHandler<ListNotificationsByUserQuery, IReadOnlyList<NotificationDto>>
{
    private readonly INotificationRepository _notificationRepository;
    private readonly IMapper _mapper;

    public ListNotificationsByUserQueryHandler(INotificationRepository notificationRepository, IMapper mapper)
    {
        _notificationRepository = notificationRepository;
        _mapper = mapper;
    }

    public async Task<Result<IReadOnlyList<NotificationDto>>> Handle(ListNotificationsByUserQuery request, CancellationToken cancellationToken)
    {
        var limit = request.Take is > 0 and <= 200 ? request.Take.Value : 50;
        var notifications = await _notificationRepository.GetUserNotificationsAsync(request.UserId, limit, cancellationToken);
        var dtos = notifications
            .Select(notification => _mapper.Map<NotificationDto>(notification))
            .ToList()
            .AsReadOnly();

        return Result.Success<IReadOnlyList<NotificationDto>>(dtos);
    }
}

