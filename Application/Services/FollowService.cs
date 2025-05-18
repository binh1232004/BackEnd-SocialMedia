// using Application.DTOs;
// using Application.Interfaces.RepositoryInterfaces;
// using Application.Interfaces.ServiceInterfaces;
// using Domain.Entities;
//
// namespace Application.Services;
//
// public class FollowService : IFollowService
// {
//     private readonly IFollowRepository _followRepository;
//     private readonly IUserRepository _userRepository;
//
//     public FollowService(IFollowRepository followRepository, IUserRepository userRepository)
//     {
//         _followRepository = followRepository;
//         _userRepository = userRepository;
//     }
//
//     public async Task<FollowResponseDto> FollowUser(string followerId, string followedId)
//     {
//         // Check if users exist
//         var follower = await _userRepository.GetUserById(followerId);
//         var followed = await _userRepository.GetUserById(followedId);
//
//         if (follower == null || followed == null)
//         {
//             return new FollowResponseDto
//             {
//                 Success = false,
//                 Message = "User not found"
//             };
//         }
//
//         // Check if already following
//         var existingFollow = await _followRepository.GetFollow(followerId, followedId);
//         if (existingFollow != null)
//         {
//             return new FollowResponseDto
//             {
//                 Success = false,
//                 Message = "Already following this user"
//             };
//         }
//
//         // Create follow record
//         var follow = new user_follow
//         {
//             follower_id = followerId,
//             followed_id = followedId,
//             followed_time = DateTime.UtcNow
//         };
//
//         await _followRepository.AddFollow(follow);
//
//         return new FollowResponseDto
//         {
//             Success = true,
//             Message = "User followed successfully",
//             Follow = new FollowDto
//             {
//                 UserId = followed.user_id,
//                 Username = followed.username,
//                 FullName = followed.full_name,
//                 Image = followed.image,
//                 FollowedTime = follow.followed_time
//             }
//         };
//     }
//
//     public async Task<FollowResponseDto> UnfollowUser(string followerId, string followedId)
//     {
//         var follow = await _followRepository.GetFollow(followerId, followedId);
//         if (follow == null)
//         {
//             return new FollowResponseDto
//             {
//                 Success = false,
//                 Message = "Not following this user"
//             };
//         }
//
//         var result = await _followRepository.RemoveFollow(follow);
//         
//         return new FollowResponseDto
//         {
//             Success = result,
//             Message = result ? "User unfollowed successfully" : "Failed to unfollow user"
//         };
//     }
//
//     public async Task<GetFollowersResponseDto> GetFollowers(string userId, int page = 1, int pageSize = 20)
//     {
//         int skip = (page - 1) * pageSize;
//         var followers = await _followRepository.GetFollowers(userId, skip, pageSize);
//         var count = await _followRepository.GetFollowersCount(userId);
//
//         var followerDtos = followers.Select(f => new FollowDto
//         {
//             UserId = f.follower.user_id,
//             Username = f.follower.username,
//             FullName = f.follower.full_name,
//             Image = f.follower.image,
//             FollowedTime = f.followed_time
//         }).ToList();
//
//         return new GetFollowersResponseDto
//         {
//             Followers = followerDtos,
//             TotalCount = count
//         };
//     }
//
//     public async Task<GetFollowingResponseDto> GetFollowing(string userId, int page = 1, int pageSize = 20)
//     {
//         int skip = (page - 1) * pageSize;
//         var following = await _followRepository.GetFollowing(userId, skip, pageSize);
//         var count = await _followRepository.GetFollowingCount(userId);
//
//         var followingDtos = following.Select(f => new FollowDto
//         {
//             UserId = f.followed.user_id,
//             Username = f.followed.username,
//             FullName = f.followed.full_name,
//             Image = f.followed.image,
//             FollowedTime = f.followed_time
//         }).ToList();
//
//         return new GetFollowingResponseDto
//         {
//             Following = followingDtos,
//             TotalCount = count
//         };
//     }
//
//     public async Task<bool> IsFollowing(string followerId, string followedId)
//     {
//         return await _followRepository.IsFollowing(followerId, followedId);
//     }
// }
