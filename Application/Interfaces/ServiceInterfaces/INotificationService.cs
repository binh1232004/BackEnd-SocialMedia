using Application.DTOs;

namespace Application.Interfaces.ServiceInterfaces;

public interface INotificationService
{
    Task CreateMessageNotificationAsync(Guid userId, Guid relatedUserId, Guid messageId);
    Task SendNotificationAsync(NotificationDto notificationDto);
}