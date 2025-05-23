using Application.DTOs;
using Application.Interfaces.RepositoryInterfaces;
using Application.Interfaces.ServiceInterfaces;
using Domain.Entities;
using Mapster;
using Microsoft.AspNetCore.SignalR;

namespace Application.Services;

public class NotificationService : INotificationService
{
    private readonly INotificationRepository _notificationRepository;
    private readonly IHubContext<Hubs.ChatHub> _hubContext;

    public NotificationService(
        INotificationRepository notificationRepository,
        IHubContext<Hubs.ChatHub> hubContext)
    {
        _notificationRepository = notificationRepository;
        _hubContext = hubContext;
    }

    public async Task CreateMessageNotificationAsync(Guid userId, Guid relatedUserId, Guid messageId)
    {
        var notification = new Notification()
        {
            NotificationId = Guid.NewGuid(),
            UserId = userId,
            Type = "NewMessage",
            RelatedUserId = relatedUserId,
            RelatedMessageId = messageId,
            NotifiedAt = DateTime.UtcNow,
            IsRead = false
        };

        await _notificationRepository.AddAsync(notification);
        var notificationDto = notification.Adapt<NotificationDto>();
        await SendNotificationAsync(notificationDto);
    }

    public async Task SendNotificationAsync(NotificationDto notificationDto)
    {
        await _hubContext.Clients.User(notificationDto.UserId.ToString())
            .SendAsync("ReceiveNotification", notificationDto);
    }
}
