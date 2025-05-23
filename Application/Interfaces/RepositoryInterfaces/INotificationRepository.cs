using Domain.Entities;

namespace Application.Interfaces.RepositoryInterfaces;

public interface INotificationRepository
{
    Task AddAsync(Notification notification);

}