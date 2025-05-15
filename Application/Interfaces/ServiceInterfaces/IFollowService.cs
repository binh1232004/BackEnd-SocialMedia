using Application.DTOs;

namespace Application.Interfaces.ServiceInterfaces;

public interface IFollowService
{
    Task<FollowResponseDto> FollowUser(string followerId, string followedId);
    Task<FollowResponseDto> UnfollowUser(string followerId, string followedId);
    Task<GetFollowersResponseDto> GetFollowers(string userId, int page = 1, int pageSize = 20);
    Task<GetFollowingResponseDto> GetFollowing(string userId, int page = 1, int pageSize = 20);
    Task<bool> IsFollowing(string followerId, string followedId);
}
