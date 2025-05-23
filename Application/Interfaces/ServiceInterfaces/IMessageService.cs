using Application.DTOs;

namespace Application.Interfaces.ServiceInterfaces;

public interface IMessageService
{
    Task<MessageDto> SendMessageAsync(CreateMessageDto createMessageDto);
    Task<PagedMessageDto> GetMessagesWithUserAsync(Guid userId, Guid receiverId, int page, int pageSize);
    Task<PagedMessageDto> GetGroupMessagesAsync(Guid userId, Guid groupChatId, int page, int pageSize);
    Task MarkMessageAsReadAsync(Guid userId, Guid messageId);
}