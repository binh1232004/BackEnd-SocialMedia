namespace Application.DTOs;

public class MessageDto
{
    public string MessageId { get; set; } = null!;
    public string SenderId { get; set; } = null!;
    public string SenderName { get; set; } = null!;
    public string? SenderImage { get; set; }
    public string? ReceiverId { get; set; }
    public string? ReceiverName { get; set; }
    public string? ReceiverImage { get; set; }
    public string? GroupChatId { get; set; }
    public string? Content { get; set; }
    public string? MediaType { get; set; }
    public string? MediaUrl { get; set; }
    public DateTime? SentTime { get; set; }
    public bool? IsRead { get; set; }
}

public class SendMessageDto
{
    public string? ReceiverId { get; set; }
    public string? GroupChatId { get; set; }
    public string? Content { get; set; }
    public string? MediaType { get; set; }
    public string? MediaUrl { get; set; }
}

public class MessageResponseDto
{
    public bool Success { get; set; }
    public string Message { get; set; } = null!;
    public MessageDto? Data { get; set; }
}

public class GetMessagesResponseDto
{
    public List<MessageDto> Messages { get; set; } = new();
    public int TotalCount { get; set; }
}

