using Domain.Entities;

namespace Application.Interfaces.RepositoryInterfaces;

public interface IMessageRepository
{
    Task<message> CreateMessage(message message);
    Task<List<message>> GetMessagesBetweenUsers(string user1Id, string user2Id, int skip, int take);
    Task<int> GetMessageCountBetweenUsers(string user1Id, string user2Id);
    Task<bool> MarkMessagesAsRead(string senderId, string receiverId);
    Task<List<message>> GetUnreadMessages(string userId);
    Task<List<user>> GetChatUsers(string userId, int skip, int take);
}

