using Application.Interfaces.RepositoryInterfaces;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class GroupChatRepository : IGroupChatRepository
{
    private readonly ApplicationDbContext _context;

    public GroupChatRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<bool> IsUserInGroupAsync(Guid userId, Guid groupChatId)
    {
        return await _context.GroupChatMembers
            .AnyAsync(m => m.GroupChatId == groupChatId && m.UserId == userId);
    }
}