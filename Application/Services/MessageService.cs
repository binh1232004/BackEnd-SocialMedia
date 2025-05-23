using Application.DTOs;
using Application.Interfaces.RepositoryInterfaces;
using Application.Interfaces.ServiceInterfaces;
using Domain.Entities;
using Mapster;
using Microsoft.AspNetCore.SignalR;



namespace Application.Services;

public class MessageService : IMessageService
{
    private readonly IMessageRepository _messageRepository;
    private readonly IGroupChatRepository _groupChatRepository;
    private readonly INotificationService _notificationService;
    private readonly IHubContext<Hubs.ChatHub> _hubContext; // Fixed namespace


    public MessageService(
        IMessageRepository messageRepository,
        IGroupChatRepository groupChatRepository,
        INotificationService notificationService,
        IHubContext<Hubs.ChatHub> hubContext)
    {
        _messageRepository = messageRepository;
        _groupChatRepository = groupChatRepository;
        _notificationService = notificationService;
        _hubContext = hubContext;
    }

    public async Task<MessageDto> SendMessageAsync(CreateMessageDto createMessageDto)
    {
        if (createMessageDto.ReceiverId == null && createMessageDto.GroupChatId == null)
            throw new ArgumentException("ReceiverId or GroupChatId must be provided.");

        if (createMessageDto.GroupChatId.HasValue)
        {
            if (!await _groupChatRepository.IsUserInGroupAsync(createMessageDto.SenderId,
                    createMessageDto.GroupChatId.Value))
                throw new UnauthorizedAccessException("User is not in this group chat.");
        }

        var message = createMessageDto.Adapt<Message>();
        message.MessageId = Guid.NewGuid();
        message.SentAt = DateTime.UtcNow;
        message.IsRead = false;

        if (createMessageDto.MediaUrls != null && createMessageDto.MediaUrls.Any())
        {
            message.Media = createMessageDto.MediaUrls.Select(url => new Media
            {
                MediaId = Guid.NewGuid(),
                MediaUrl = url,
                MediaType = url.EndsWith(".jpg") || url.EndsWith(".png") ? "image" : "video",
                UploadedAt = DateTime.UtcNow,
                UploadedBy = createMessageDto.SenderId,
                MessageId = message.MessageId
            }).ToList();
        }

        await _messageRepository.AddAsync(message);

        var messageDto = message.Adapt<MessageDto>();
        if (createMessageDto.ReceiverId.HasValue)
        {
            await _notificationService.CreateMessageNotificationAsync(createMessageDto.ReceiverId.Value,
                createMessageDto.SenderId, message.MessageId);
            await _hubContext.Clients.User(createMessageDto.ReceiverId.Value.ToString())
                .SendAsync("ReceiveMessage", messageDto);
        }
        else if (createMessageDto.GroupChatId.HasValue)
        {
            await _hubContext.Clients.Group(createMessageDto.GroupChatId.Value.ToString())
                .SendAsync("ReceiveGroupMessage", messageDto);
        }

        return messageDto;
    }

    public async Task<PagedMessageDto> GetMessagesWithUserAsync(Guid userId, Guid receiverId, int page, int pageSize)
    {
        var messages = await _messageRepository.GetByUserAsync(userId, receiverId, page, pageSize);
        var messageDtos = messages.Adapt<List<MessageDto>>();
        return new PagedMessageDto
        {
            Messages = messageDtos,
            TotalCount = messageDtos.Count,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<PagedMessageDto> GetGroupMessagesAsync(Guid userId, Guid groupChatId, int page, int pageSize)
    {
        if (!await _groupChatRepository.IsUserInGroupAsync(userId, groupChatId))
            throw new UnauthorizedAccessException("User is not in this group chat.");

        var messages = await _messageRepository.GetByGroupAsync(groupChatId, page, pageSize);
        var messageDtos = messages.Adapt<List<MessageDto>>();
        return new PagedMessageDto
        {
            Messages = messageDtos,
            TotalCount = messageDtos.Count,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task MarkMessageAsReadAsync(Guid userId, Guid messageId)
    {
        var message = await _messageRepository.GetByIdAsync(messageId);
        if (message == null || (message.ReceiverId != userId && message.GroupChatId == null))
            throw new UnauthorizedAccessException("Invalid message or user.");

        if (message.IsRead != true)
        {
            message.IsRead = true;
            await _messageRepository.UpdateAsync(message);
            await _hubContext.Clients.User(message.SenderId.ToString()).SendAsync("MessageRead", messageId);
        }
    }
}