namespace Application.Interfaces.ServiceInterfaces;

public interface ICacheService
{
    Task SetUserOnlineAsync(string userId);
    Task SetUserOfflineAsync(string userId);
    Task<bool> IsUserOnlineAsync(string userId);
}