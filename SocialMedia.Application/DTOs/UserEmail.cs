using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SocialMedia.Application.DTOs
{
    public class UserEmail
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
    }
}
