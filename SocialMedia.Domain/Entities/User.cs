using System.ComponentModel.DataAnnotations;

namespace SocialMedia.Domain.Entities
{
    public class User
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid(); // UUID làm khóa chính

        [Required, MaxLength(100)]
        public string FullName { get; set; }

        public string? Image { get; set; } // URL ảnh đại diện

        public DateTime? DoB { get; set; } // Ngày sinh

        [MaxLength(10)]
        public string? Gender { get; set; } // Giới tính

        [MaxLength(500)]
        public string? Biography { get; set; } // Tiểu sử

        [Required, EmailAddress]
        public string Email { get; set; } // Email

        [Required]
        public string PasswordHash { get; private set; } // Mật khẩu mã hóa

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow; // Ngày tạo tài khoản
        public DateTime? LastLogin { get; set; } // Lần đăng nhập cuối
        public bool IsActive { get; set; } = true; // Trạng thái tài khoản

        // Mã hóa mật khẩu trước khi lưu
        public void SetPassword(string password)
        {
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(password);
        }

        // Xác minh mật khẩu khi đăng nhập
        public bool VerifyPassword(string password)
        {
            return BCrypt.Net.BCrypt.Verify(password, PasswordHash);
        }
    }
}
