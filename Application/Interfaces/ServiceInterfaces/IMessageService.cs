using Application.DTOs;

namespace Application.Interfaces.ServiceInterfaces;

public interface IMessageService
{
    Task<MessageDto> SaveAndBroadcastMessageAsync(MessageDto messageDto);
    Task<List<string>> GetGroupMemberIdsAsync(string group_chat_id);
}