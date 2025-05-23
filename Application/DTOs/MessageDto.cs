using System.ComponentModel.DataAnnotations;

namespace Application.DTOs;

public class MessageDto
{
    public Guid MessageId { get; set; }
    public Guid SenderId { get; set; }
    public string SenderUsername { get; set; }
    public Guid? ReceiverId { get; set; }
    public Guid? GroupChatId { get; set; }
    public string Content { get; set; }
    public DateTime? SentAt { get; set; }
    public bool IsRead { get; set; }
    public List<MediaDto> Media { get; set; }
}

public class CreateMessageDto
{
    public Guid SenderId { get; set; }
    public Guid? ReceiverId { get; set; }
    public Guid? GroupChatId { get; set; }
    // [Required(ErrorMessage = "The Content field is required.")]
    public string Content { get; set; } // Chữ in hoa
    public List<string> MediaUrls { get; set; } = [];
}

public class MediaDto
{
    public Guid MediaId { get; set; }
    public string MediaUrl { get; set; }
    public string MediaType { get; set; }
}

public class PagedMessageDto
{
    public List<MessageDto> Messages { get; set; }
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
}