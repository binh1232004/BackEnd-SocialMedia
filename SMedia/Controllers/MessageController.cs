using Application.DTOs;
using Application.Interfaces.ServiceInterfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace SMedia.Controllers;

[Route("api/messages")]
[ApiController]
[Authorize]
public class MessageController : ControllerBase
{
    private readonly IMessageService _messageService;

    public MessageController(IMessageService messageService)
    {
        _messageService = messageService;
    }

    [HttpPost("send-message")]
    public async Task<ActionResult<MessageDto>> SendMessage(
        [FromQuery] Guid senderId,
        [FromQuery] Guid? receiverId,
        [FromQuery] Guid? groupChatId,
        [FromQuery] string content,
        [FromQuery] List<string> mediaUrls = null)
    {
        Console.WriteLine(
            $"Received: SenderId={senderId}, ReceiverId={receiverId}, GroupChatId={groupChatId}, Content={content}, MediaUrls={(mediaUrls != null ? string.Join(",", mediaUrls) : "null")}");

        if (string.IsNullOrEmpty(content))
        {
            return BadRequest(new { errors = new { Content = new[] { "The Content field is required." } } });
        }

        var createMessageDto = new CreateMessageDto
        {
            SenderId = senderId,
            ReceiverId = receiverId,
            GroupChatId = groupChatId,
            Content = content,
            MediaUrls = mediaUrls ?? new List<string>()
        };

        var message = await _messageService.SendMessageAsync(createMessageDto);
        return Ok(message);
    }

    [HttpGet("user/{receiverId}")]
    public async Task<ActionResult<PagedMessageDto>> GetMessagesWithUser(Guid receiverId, [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        try
        {
            var userIdClaim = User.FindFirst("user_id")?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized(new { error = "Invalid user authentication." });
            }

            var messages = await _messageService.GetMessagesWithUserAsync(userId, receiverId, page, pageSize);
            return Ok(messages);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in GetMessagesWithUser: {ex.Message}");
            return StatusCode(500, new { error = "An error occurred while fetching messages." });
        }
    }

    [HttpGet("group/{groupChatId}")]
    public async Task<ActionResult<PagedMessageDto>> GetGroupMessages(Guid groupChatId, [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        try
        {
            var userIdClaim = User.FindFirst("user_id")?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized(new { error = "Invalid user authentication." });
            }

            var messages = await _messageService.GetGroupMessagesAsync(userId, groupChatId, page, pageSize);
            return Ok(messages);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in GetGroupMessages: {ex.Message}");
            return StatusCode(500, new { error = "An error occurred while fetching group messages." });
        }
    }

    [HttpPost("{messageId}/read")]
    public async Task<IActionResult> MarkMessageAsRead(Guid messageId)
    {
        try
        {
            var userIdClaim = User.FindFirst("user_id")?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized(new { error = "Invalid user authentication." });
            }

            await _messageService.MarkMessageAsReadAsync(userId, messageId);
            return NoContent();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in MarkMessageAsRead: {ex.Message}");
            return StatusCode(500, new { error = "An error occurred while marking the message as read." });
        }
    }
}