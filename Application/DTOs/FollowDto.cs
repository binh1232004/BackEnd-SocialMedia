using System;

namespace Application.DTOs;

public class FollowDto
{
    public string UserId { get; set; } = null!;
    public string Username { get; set; } = null!;
    public string? FullName { get; set; }
    public string? Image { get; set; }
    public DateTime? FollowedTime { get; set; }
}

public class FollowRequestDto
{
    public string UserId { get; set; } = null!;
}

public class FollowResponseDto
{
    public bool Success { get; set; }
    public string Message { get; set; } = null!;
    public FollowDto? Follow { get; set; }
}

public class GetFollowersResponseDto
{
    public List<FollowDto> Followers { get; set; } = new();
    public int TotalCount { get; set; }
}

public class GetFollowingResponseDto
{
    public List<FollowDto> Following { get; set; } = new();
    public int TotalCount { get; set; }
}
