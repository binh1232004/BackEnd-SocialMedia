using System;
using Microsoft.AspNetCore.Http;

namespace Application.DTOs;

public class UserUpdateDto
{
    public string? fullName { get; set; }
    public string? intro { get; set; }
    public DateOnly? birthday { get; set; }
    public string? gender { get; set; }
    public string? image { get; set; }
    public IFormFile? imageFile { get; set; } // For file upload
}
