using System.Security.Claims;
using Application.DTOs;
using Application.Interfaces.ServiceInterfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Filters;

namespace SMedia.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
[Produces("application/json")]
public class FollowController : ControllerBase
{
    private readonly IFollowService _followService;

    public FollowController(IFollowService followService)
    {
        _followService = followService;
    }
    
    /// <summary>
    /// Follow another user
    /// </summary>
    /// <param name="userId">ID of the user to follow</param>
    /// <returns>Information about the follow operation status</returns>
    /// <response code="200">Follow operation successful</response>
    /// <response code="400">Failed to follow (e.g., cannot follow yourself)</response>
    /// <response code="401">Unauthorized access</response>
    [HttpPost("follow/{userId}")]
    [SwaggerResponseExample(200, typeof(SMedia.SwaggerExamples.Follow.FollowResponseDtoExample))]
    [SwaggerResponseExample(400, typeof(SMedia.SwaggerExamples.Follow.FollowResponseDtoExample))]
    public async Task<ActionResult<FollowResponseDto>> FollowUser(string userId)
    {
        var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (currentUserId == null)
        {
            return Unauthorized();
        }

        if (userId == currentUserId)
        {
            return BadRequest(new FollowResponseDto
            {
                Success = false,
                Message = "Cannot follow yourself"
            });
        }

        var result = await _followService.FollowUser(currentUserId, userId);
        
        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }    /// <summary>
    /// Unfollow a user that you are currently following
    /// </summary>
    /// <param name="userId">ID of the user to unfollow</param>
    /// <returns>Information about the unfollow operation status</returns>
    /// <response code="200">Unfollow operation successful</response>
    /// <response code="400">Failed to unfollow</response>
    /// <response code="401">Unauthorized access</response>
    [HttpDelete("unfollow/{userId}")]
    [SwaggerResponseExample(200, typeof(SMedia.SwaggerExamples.Follow.FollowResponseDtoExample))]
    public async Task<ActionResult<FollowResponseDto>> UnfollowUser(string userId)
    {
        var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (currentUserId == null)
        {
            return Unauthorized();
        }

        var result = await _followService.UnfollowUser(currentUserId, userId);
        
        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }    /// <summary>
    /// Get list of users who follow the specified user
    /// </summary>
    /// <param name="userId">ID of the user whose followers to retrieve</param>
    /// <param name="page">Page number (1-based)</param>
    /// <param name="pageSize">Number of results per page</param>
    /// <returns>List of followers with pagination information</returns>
    /// <response code="200">Returns the list of followers</response>
    [HttpGet("followers/{userId}")]
    [SwaggerResponseExample(200, typeof(SMedia.SwaggerExamples.Follow.GetFollowersResponseDtoExample))]
    public async Task<ActionResult<GetFollowersResponseDto>> GetFollowers(
        string userId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var result = await _followService.GetFollowers(userId, page, pageSize);
        return Ok(result);
    }    /// <summary>
    /// Get list of users that the specified user follows
    /// </summary>
    /// <param name="userId">ID of the user whose following list to retrieve</param>
    /// <param name="page">Page number (1-based)</param>
    /// <param name="pageSize">Number of results per page</param>
    /// <returns>List of users being followed with pagination information</returns>
    /// <response code="200">Returns the list of users being followed</response>
    [HttpGet("following/{userId}")]
    [SwaggerResponseExample(200, typeof(SMedia.SwaggerExamples.Follow.GetFollowingResponseDtoExample))]
    public async Task<ActionResult<GetFollowingResponseDto>> GetFollowing(
        string userId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var result = await _followService.GetFollowing(userId, page, pageSize);
        return Ok(result);
    }    /// <summary>
    /// Check if the current user is following the specified user
    /// </summary>
    /// <param name="userId">ID of the user to check follow status for</param>
    /// <returns>Boolean indicating whether the current user follows the specified user</returns>
    /// <response code="200">Returns the follow status</response>
    /// <response code="401">Unauthorized access</response>
    [HttpGet("is-following/{userId}")]
    public async Task<ActionResult<bool>> IsFollowing(string userId)
    {
        var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (currentUserId == null)
        {
            return Unauthorized();
        }

        var result = await _followService.IsFollowing(currentUserId, userId);
        return Ok(result);
    }
}
