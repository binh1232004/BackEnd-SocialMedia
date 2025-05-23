using Application.Interfaces.ServiceInterfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Application.DTOs;
using Microsoft.Extensions.Logging;

namespace Application.Hubs;

[Authorize]
public class ChatHub : Hub
{
    private readonly IMessageService _messageService;
    private readonly ICacheService _cacheService;
    private readonly ILogger<ChatHub> _logger;
    private readonly IGroupService _groupService;

    public ChatHub(IMessageService messageService, ICacheService cacheService, ILogger<ChatHub> logger,
        IGroupService groupService)
    {
        _messageService = messageService;
        _cacheService = cacheService;
        _logger = logger;
        _groupService = groupService;
    }

    public override async Task OnConnectedAsync()
    {
        var userId = Context.User?.FindFirst("user_id")?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            throw new HubException("User not authenticated.");
        }

        await _cacheService.SetUserOnlineAsync(userId);
        await Clients.All.SendAsync("UserOnline", userId);
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userId = Context.User?.FindFirst("user_id")?.Value;
        if (!string.IsNullOrEmpty(userId))
        {
            await _cacheService.SetUserOfflineAsync(userId);
            await Clients.All.SendAsync("UserOffline", userId);
        }

        await base.OnDisconnectedAsync(exception);
    }

    public async Task SendMessageToUser(string receiverId, string content)
    {
        var userId = Context.User?.FindFirst("user_id")?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            throw new HubException("User not authenticated.");
        }

        if (string.IsNullOrEmpty(content))
        {
            throw new HubException("Content cannot be empty.");
        }

        try
        {
            var messageDto = await _messageService.SendMessageAsync(new CreateMessageDto
            {
                SenderId = Guid.Parse(userId),
                ReceiverId = Guid.Parse(receiverId),
                Content = content.Length > 1000 ? content.Substring(0, 1000) : content
            });

            // Gửi tin nhắn đến người nhận và người gửi
            await Clients.User(receiverId).SendAsync("ReceiveMessage", messageDto);
            await Clients.Caller.SendAsync("ReceiveMessage", messageDto);
        }
        catch (Exception ex)
        {
            // Sửa: Thêm logging bằng ILogger, giữ Console.WriteLine để debug
            _logger.LogError(ex, "Error in SendMessageToUser: {Message}", ex.Message);
            Console.WriteLine($"Error in SendMessageToUser: {ex.Message}");
            throw new HubException($"Failed to send message: {ex.Message}");
        }
    }

    public async Task SendMessageToGroup(string groupChatId, string content)
    {
        var userId = Context.User?.FindFirst("user_id")?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            throw new HubException("User not authenticated.");
        }

        if (string.IsNullOrEmpty(content))
        {
            throw new HubException("Content cannot be empty.");
        }

        try
        {
            var messageDto = await _messageService.SendMessageAsync(new CreateMessageDto
            {
                SenderId = Guid.Parse(userId),
                GroupChatId = Guid.Parse(groupChatId),
                Content = content.Length > 1000 ? content.Substring(0, 1000) : content
            });

            await Clients.Group(groupChatId).SendAsync("ReceiveGroupMessage", messageDto);
        }
        catch (Exception ex)
        {
            // Sửa: Thêm logging bằng ILogger, giữ Console.WriteLine để debug
            _logger.LogError(ex, "Error in SendMessageToGroup: {Message}", ex.Message);
            Console.WriteLine($"Error in SendMessageToGroup: {ex.Message}");
            throw new HubException($"Failed to send message to group: {ex.Message}");
        }
    }

    public async Task SendTyping(string receiverId, bool isTyping)
    {
        await Clients.User(receiverId).SendAsync("Typing", Context.User?.FindFirst("user_id")?.Value, isTyping);
    }

    public async Task SendGroupTyping(string groupChatId, bool isTyping)
    {
        await Clients.Group(groupChatId).SendAsync("GroupTyping", Context.User?.FindFirst("user_id")?.Value, isTyping);
    }

    public async Task JoinGroupChat(string groupChatId)
    {
        // Sửa: Thêm kiểm tra quyền truy cập vào group chat
        var userId = Context.User?.FindFirst("user_id")?.Value;
        if (string.IsNullOrEmpty(userId) ||
            !await _groupService.IsMemberOfGroupAsync(Guid.Parse(userId), Guid.Parse(groupChatId)))
        {
            _logger.LogWarning("Unauthorized attempt to join group chat {GroupChatId} by user {UserId}", groupChatId,
                userId);
            throw new HubException("User is not authorized to join this group chat.");
        }

        await Groups.AddToGroupAsync(Context.ConnectionId, groupChatId);
        _logger.LogInformation("User {UserId} joined group chat {GroupChatId}", userId, groupChatId);
    }

    public async Task LeaveGroupChat(string groupChatId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupChatId);
        // Sửa: Thêm logging khi rời nhóm
        var userId = Context.User?.FindFirst("user_id")?.Value;
        _logger.LogInformation("User {UserId} left group chat {GroupChatId}", userId, groupChatId);
    }
}