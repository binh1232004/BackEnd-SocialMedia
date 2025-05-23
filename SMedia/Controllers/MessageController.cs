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

    [HttpPost]
    public async Task<ActionResult<MessageDto>> SendMessage([FromBody] CreateMessageDto createMessageDto)
    {
        var message = await _messageService.SendMessageAsync(createMessageDto);
        return Ok(message);
    }

    [HttpGet("user/{receiverId}")]
    public async Task<ActionResult<PagedMessageDto>> GetMessagesWithUser(Guid receiverId, [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var userId = Guid.Parse(User.FindFirst("user_id")?.Value);
        var messages = await _messageService.GetMessagesWithUserAsync(userId, receiverId, page, pageSize);
        return Ok(messages);
    }

    [HttpGet("group/{groupChatId}")]
    public async Task<ActionResult<PagedMessageDto>> GetGroupMessages(Guid groupChatId, [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var userId = Guid.Parse(User.FindFirst("user_id")?.Value);
        var messages = await _messageService.GetGroupMessagesAsync(userId, groupChatId, page, pageSize);
        return Ok(messages);
    }

    [HttpPost("{messageId}/read")]
    public async Task<IActionResult> MarkMessageAsRead(Guid messageId)
    {
        var userId = Guid.Parse(User.FindFirst("user_id")?.Value);
        await _messageService.MarkMessageAsReadAsync(userId, messageId);
        return NoContent();
    }
}