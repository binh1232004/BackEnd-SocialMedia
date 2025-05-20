using static Application.DTOs.PostDtos;
using Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Application.Interfaces.ServiceInterfaces;

namespace SMedia.Controllers
{
    [Route("api/group-posts")]
    [ApiController]
    [Authorize]
    public class GroupPostsController : ControllerBase
    {
        private readonly IPostService _postService;

        public GroupPostsController(IPostService postService)
        {
            _postService = postService;
        }

        [HttpPost]
        public async Task<ActionResult<PostDto>> CreateGroupPost([FromBody] GroupPostCreateDto postDto)
        {
            var userId = Guid.Parse(User.FindFirst("user_id")?.Value ?? throw new UnauthorizedAccessException("Invalid token."));
            var post = await _postService.CreateGroupPostAsync(postDto, userId);
            return CreatedAtAction(nameof(GetGroupPosts), new { groupId = post.GroupId, page = 1, pageSize = 10 }, post);
        }

        [HttpGet("{groupId}")]
        public async Task<ActionResult<PostDto[]>> GetGroupPosts(Guid groupId, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var currentUserId = Guid.Parse(User.FindFirst("user_id")?.Value ?? throw new UnauthorizedAccessException("Invalid token."));
            var posts = await _postService.GetGroupPostsAsync(groupId, page, pageSize, currentUserId);
            return Ok(posts);
        }
        
        [HttpPost("{groupId}/approve")]
        public async Task<ActionResult<PostDto>> ApproveGroupPost(Guid groupId, [FromBody] GroupPostApproveDto approveDto)
        {
            try
            {
                if (!Guid.TryParse(User.FindFirst("user_id")?.Value, out var adminId))
                    return Unauthorized(new { Error = "Invalid token: user_id is missing or invalid." });

                var post = await _postService.ApproveGroupPostAsync(groupId, approveDto, adminId);
                return Ok(post);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { Error = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { Error = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { Error = ex.Message });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error approving post {approveDto.PostId} for group {groupId}: {ex.Message}");
                return StatusCode(500, new { Error = "An error occurred while approving the post." });
            }
        }
    }
}