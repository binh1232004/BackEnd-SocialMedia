namespace SocialMedia.Application.DTOs
{
    public class UserDto
    {
        public Guid Id { get; set; }
        public string FullName { get; set; }
        public string? Image { get; set; }
        public DateTime? DoB { get; set; }
        public string? Gender { get; set; }
        public string? Biography { get; set; }
        public string Email { get; set; }
        public bool IsActive { get; set; }
    }
}
