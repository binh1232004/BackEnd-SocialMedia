using Application.Interfaces.RepositoryInterfaces;
using Domain.Entities;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class MessageRepository : IMessageRepository
{
    private readonly ApplicationDbContext _context;

    public MessageRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<message> CreateMessage(message message)
    {
        _context.messages.Add(message);
        await _context.SaveChangesAsync();
        return message;
    }

    public async Task<List<message>> GetMessagesBetweenUsers(string user1Id, string user2Id, int skip, int take)
    {
        return await _context.messages
            .Include(m => m.sender)
            .Include(m => m.receiver)
            .Where(m => 
                (m.sender_id == user1Id && m.receiver_id == user2Id) || 
                (m.sender_id == user2Id && m.receiver_id == user1Id))
            .OrderByDescending(m => m.sent_time)
            .Skip(skip)
            .Take(take)
            .ToListAsync();
    }

    public async Task<int> GetMessageCountBetweenUsers(string user1Id, string user2Id)
    {
        return await _context.messages
            .CountAsync(m => 
                (m.sender_id == user1Id && m.receiver_id == user2Id) || 
                (m.sender_id == user2Id && m.receiver_id == user1Id));
    }

    public async Task<bool> MarkMessagesAsRead(string senderId, string receiverId)
    {
        var unreadMessages = await _context.messages
            .Where(m => m.sender_id == senderId && m.receiver_id == receiverId && m.is_read == false)
            .ToListAsync();

        if (!unreadMessages.Any())
            return false;

        foreach (var message in unreadMessages)
        {
            message.is_read = true;
        }

        return await _context.SaveChangesAsync() > 0;
    }

    public async Task<List<message>> GetUnreadMessages(string userId)
    {
        return await _context.messages
            .Include(m => m.sender)
            .Where(m => m.receiver_id == userId && m.is_read == false)
            .OrderByDescending(m => m.sent_time)
            .ToListAsync();
    }

    public async Task<List<user>> GetChatUsers(string userId, int skip, int take)
    {
        // Get unique users that have exchanged messages with the current user
        var userIds = await _context.messages
            .Where(m => m.sender_id == userId || m.receiver_id == userId)
            .Select(m => m.sender_id == userId ? m.receiver_id : m.sender_id)
            .Distinct()
            .Skip(skip)
            .Take(take)
            .ToListAsync();

        // Filter out null values
        userIds = userIds.Where(id => id != null).Cast<string>().ToList();

        // Get user objects for these IDs
        return await _context.users
            .Where(u => userIds.Contains(u.user_id))
            .ToListAsync();
    }
}
