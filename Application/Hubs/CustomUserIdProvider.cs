using Microsoft.AspNetCore.SignalR;

namespace Application.Hubs;

public class CustomUserIdProvider : IUserIdProvider
{
    public string? GetUserId(HubConnectionContext connection)
    {
        // Lấy user_id từ token
        return connection.User?.FindFirst("user_id")?.Value;
    }
}