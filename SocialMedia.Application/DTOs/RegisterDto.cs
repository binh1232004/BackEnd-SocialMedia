using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SocialMedia.Application.DTOs
{
    public class RegisterDto
    {
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

        [Required, MinLength(6)]
        public string Password { get; set; } // Mật khẩu
    }
}
