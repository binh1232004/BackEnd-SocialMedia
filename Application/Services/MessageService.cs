using Application.DTOs;
using Application.Interfaces.RepositoryInterfaces;
using Application.Interfaces.ServiceInterfaces;
using Domain.Entities;
using Mapster;

namespace Application.Services;

public class MessageService : IMessageService
{
    private readonly IMessageRepository _messageRepository;

    public MessageService(IMessageRepository messageRepository)
    {
        _messageRepository = messageRepository;
    }

    public async Task<MessageDto> SaveAndBroadcastMessageAsync(MessageDto messageDto)
    {
        // Ánh xạ MessageDto thành entity message
        var message = messageDto.Adapt<message>();

        // Lưu tin nhắn vào database
        await _messageRepository.SaveMessageAsync(message);

        // Trả về DTO đã lưu
        return message.Adapt<MessageDto>();
    }

    public async Task<List<string>> GetGroupMemberIdsAsync(string group_chat_id)
    {
        if (string.IsNullOrEmpty(group_chat_id))
        {
            throw new ArgumentNullException(nameof(group_chat_id), "Group chat ID cannot be null or empty.");
        }

        return await _messageRepository.GetGroupMemberIdsAsync(group_chat_id);
    }
}