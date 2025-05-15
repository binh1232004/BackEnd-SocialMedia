using Domain.Entities;

namespace Application.Interfaces.RepositoryInterfaces;

public interface IFollowRepository
{
    Task<user_follow?> GetFollow(string followerId, string followedId);
    Task<user_follow> AddFollow(user_follow follow);
    Task<bool> RemoveFollow(user_follow follow);
    Task<List<user_follow>> GetFollowers(string userId, int skip, int take);
    Task<int> GetFollowersCount(string userId);
    Task<List<user_follow>> GetFollowing(string userId, int skip, int take);
    Task<int> GetFollowingCount(string userId);
    Task<bool> IsFollowing(string followerId, string followedId);
}
