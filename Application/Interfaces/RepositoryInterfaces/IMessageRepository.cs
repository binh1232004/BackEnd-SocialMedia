using Domain.Entities;

namespace Application.Interfaces.RepositoryInterfaces;

public interface IMessageRepository
{
    Task<Message> AddAsync(Message message);
    Task<IEnumerable<Message>> GetByUserAsync(Guid senderId, Guid receiverId, int page, int pageSize);
    Task<IEnumerable<Message>> GetByGroupAsync(Guid groupChatId, int page, int pageSize);
    Task<Message> GetByIdAsync(Guid messageId);
    Task UpdateAsync(Message message);
}