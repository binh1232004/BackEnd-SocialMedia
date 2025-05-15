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
public class MessageController : ControllerBase
{
    private readonly IMessageService _messageService;
    private readonly IFollowService _followService;

    public MessageController(IMessageService messageService, IFollowService followService)
    {
        _messageService = messageService;
        _followService = followService;
    }

    /// <summary>
    /// Send a message to another user
    /// </summary>
    /// <param name="messageDto">Message details including recipient and content</param>
    /// <returns>Information about the sent message</returns>
    /// <response code="200">Message sent successfully</response>
    /// <response code="400">Message could not be sent (e.g., due to follow restrictions)</response>
    /// <response code="401">Unauthorized</response>
    [HttpPost("send")]
    [SwaggerRequestExample(typeof(SendMessageDto), typeof(SMedia.SwaggerExamples.Message.SendMessageDtoExample))]
    [SwaggerResponseExample(200, typeof(SMedia.SwaggerExamples.Message.MessageResponseDtoExample))]
    public async Task<ActionResult<MessageResponseDto>> SendMessage(SendMessageDto messageDto)
    {
        var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (currentUserId == null)
        {
            return Unauthorized();
        }

        // Check if the user is sending to someone they follow
        if (!string.IsNullOrEmpty(messageDto.ReceiverId))
        {
            bool isFollowing = await _followService.IsFollowing(currentUserId, messageDto.ReceiverId);
            bool isFollower = await _followService.IsFollowing(messageDto.ReceiverId, currentUserId);
            
            if (!isFollowing && !isFollower)
            {
                return BadRequest(new MessageResponseDto
                {
                    Success = false,
                    Message = "You can only message users who you follow or who follow you"
                });
            }
        }

        var result = await _messageService.SendMessage(currentUserId, messageDto);
        
        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }    /// <summary>
    /// Get message conversation history with a specific user
    /// </summary>
    /// <param name="userId">The ID of the user to get conversation with</param>
    /// <param name="page">Page number (1-based)</param>
    /// <param name="pageSize">Number of messages per page</param>
    /// <returns>List of messages between the current user and the specified user</returns>
    /// <response code="200">Returns the conversation history</response>
    /// <response code="401">If the user is not authorized</response>
    [HttpGet("conversation/{userId}")]
    [SwaggerResponseExample(200, typeof(SMedia.SwaggerExamples.Message.GetMessagesResponseDtoExample))]
    public async Task<ActionResult<GetMessagesResponseDto>> GetConversation(
        string userId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (currentUserId == null)
        {
            return Unauthorized();
        }

        // Auto-mark messages as read when viewing conversation
        await _messageService.MarkMessagesAsRead(userId, currentUserId);

        var result = await _messageService.GetMessagesBetweenUsers(currentUserId, userId, page, pageSize);
        return Ok(result);
    }

    [HttpPost("mark-read/{userId}")]
    public async Task<ActionResult<bool>> MarkMessagesAsRead(string userId)
    {
        var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (currentUserId == null)
        {
            return Unauthorized();
        }

        var result = await _messageService.MarkMessagesAsRead(userId, currentUserId);
        return Ok(result);
    }    [HttpGet("unread")]
    [SwaggerResponseExample(200, typeof(SMedia.SwaggerExamples.Message.MessageDtoExample))]
    public async Task<ActionResult<List<MessageDto>>> GetUnreadMessages()
    {
        var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (currentUserId == null)
        {
            return Unauthorized();
        }

        var result = await _messageService.GetUnreadMessages(currentUserId);
        return Ok(result);
    }

    [HttpGet("chat-users")]
    public async Task<ActionResult<List<object>>> GetChatUsers(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (currentUserId == null)
        {
            return Unauthorized();
        }

        var users = await _messageService.GetChatUsers(currentUserId, page, pageSize);
        
        // Map to simplified response
        var result = users.Select(u => new
        {
            userId = u.user_id,
            username = u.username,
            fullName = u.full_name,
            image = u.image
        }).ToList();
        
        return Ok(result);
    }
}
