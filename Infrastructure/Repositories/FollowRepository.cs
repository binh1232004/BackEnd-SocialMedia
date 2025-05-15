using Application.Interfaces.RepositoryInterfaces;
using Domain.Entities;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class FollowRepository : IFollowRepository
{
    private readonly ApplicationDbContext _context;

    public FollowRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<user_follow?> GetFollow(string followerId, string followedId)
    {
        return await _context.user_follows
            .FirstOrDefaultAsync(f => f.follower_id == followerId && f.followed_id == followedId);
    }

    public async Task<user_follow> AddFollow(user_follow follow)
    {
        _context.user_follows.Add(follow);
        await _context.SaveChangesAsync();
        return follow;
    }

    public async Task<bool> RemoveFollow(user_follow follow)
    {
        _context.user_follows.Remove(follow);
        return await _context.SaveChangesAsync() > 0;
    }

    public async Task<List<user_follow>> GetFollowers(string userId, int skip, int take)
    {
        return await _context.user_follows
            .Include(f => f.follower)
            .Where(f => f.followed_id == userId)
            .OrderByDescending(f => f.followed_time)
            .Skip(skip)
            .Take(take)
            .ToListAsync();
    }

    public async Task<int> GetFollowersCount(string userId)
    {
        return await _context.user_follows
            .CountAsync(f => f.followed_id == userId);
    }

    public async Task<List<user_follow>> GetFollowing(string userId, int skip, int take)
    {
        return await _context.user_follows
            .Include(f => f.followed)
            .Where(f => f.follower_id == userId)
            .OrderByDescending(f => f.followed_time)
            .Skip(skip)
            .Take(take)
            .ToListAsync();
    }

    public async Task<int> GetFollowingCount(string userId)
    {
        return await _context.user_follows
            .CountAsync(f => f.follower_id == userId);
    }

    public async Task<bool> IsFollowing(string followerId, string followedId)
    {
        return await _context.user_follows
            .AnyAsync(f => f.follower_id == followerId && f.followed_id == followedId);
    }
}
