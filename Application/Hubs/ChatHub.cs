using Application.Interfaces.ServiceInterfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace Application.Hubs;

[Authorize]
public class ChatHub : Hub
{
    private readonly IMessageService _messageService;
    private readonly ICacheService _cacheService;

    public ChatHub(IMessageService messageService, ICacheService cacheService)
    {
        _messageService = messageService;
        _cacheService = cacheService;
    }

    public override async Task OnConnectedAsync()
    {
        var userId = Context.User.FindFirst("user_id")?.Value;
        await _cacheService.SetUserOnlineAsync(userId);
        await Clients.All.SendAsync("UserOnline", userId);
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception exception)
    {
        var userId = Context.User.FindFirst("user_id")?.Value;
        await _cacheService.SetUserOfflineAsync(userId);
        await Clients.All.SendAsync("UserOffline", userId);
        await base.OnDisconnectedAsync(exception);
    }

    public async Task SendMessageToUser(string receiverId, string content)
    {
        var userId = Context.User.FindFirst("user_id")?.Value;
        var messageDto = await _messageService.SendMessageAsync(new Application.DTOs.CreateMessageDto
        {
            SenderId = Guid.Parse(userId),
            ReceiverId = Guid.Parse(receiverId),
            Content = content
        });
        await Clients.User(receiverId).SendAsync("ReceiveMessage", messageDto);
        await Clients.Caller.SendAsync("ReceiveMessage", messageDto);
    }

    public async Task SendMessageToGroup(string groupChatId, string content)
    {
        var userId = Context.User.FindFirst("user_id")?.Value;
        var messageDto = await _messageService.SendMessageAsync(new Application.DTOs.CreateMessageDto
        {
            SenderId = Guid.Parse(userId),
            GroupChatId = Guid.Parse(groupChatId),
            Content = content
        });
        await Clients.Group(groupChatId).SendAsync("ReceiveGroupMessage", messageDto);
    }

    public async Task SendTyping(string receiverId, bool isTyping)
    {
        await Clients.User(receiverId).SendAsync("Typing", Context.User.FindFirst("user_id")?.Value, isTyping);
    }

    public async Task SendGroupTyping(string groupChatId, bool isTyping)
    {
        await Clients.Group(groupChatId).SendAsync("GroupTyping", Context.User.FindFirst("user_id")?.Value, isTyping);
    }

    public async Task JoinGroupChat(string groupChatId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, groupChatId);
    }

    public async Task LeaveGroupChat(string groupChatId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupChatId);
    }
}