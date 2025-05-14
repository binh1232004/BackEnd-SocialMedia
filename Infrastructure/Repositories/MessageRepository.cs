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

    public async Task SaveMessageAsync(message message)
    {
        if (message == null)
        {
            throw new ArgumentNullException(nameof(message), "Message cannot be null.");
        }

        // Đảm bảo các trường bắt buộc không null
        message.message_id ??= Guid.NewGuid().ToString();
        message.sent_time ??= DateTime.UtcNow;
        message.is_read ??= false;

        await _context.messages.AddAsync(message);
        await _context.SaveChangesAsync();
    }

    public async Task<List<string>> GetGroupMemberIdsAsync(string group_chat_id)
    {
        if (string.IsNullOrEmpty(group_chat_id))
        {
            throw new ArgumentNullException(nameof(group_chat_id), "Group chat ID cannot be null or empty.");
        }

        return await _context.group_chat_members
            .Where(m => m.group_chat_id == group_chat_id)
            .Select(m => m.user_id)
            .ToListAsync();
    }
    public async Task<List<string>> GetBlockedUserIdsAsync(string user_id)
    {
        return await _context.user_blocks
            .Where(b => b.blocker_id == user_id)
            .Select(b => b.blocked_id)
            .ToListAsync();
    }

    public async Task<List<user>> SearchUsersAsync(string searchTerm, string currentUserId, List<string> blockedUsers)
    {
        return await _context.users
            .Where(u => u.user_id != currentUserId && 
                        !blockedUsers.Contains(u.user_id) &&
                        (u.username.Contains(searchTerm) || u.full_name.Contains(searchTerm)))
            .ToListAsync();
    }
}