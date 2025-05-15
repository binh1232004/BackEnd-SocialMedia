using Application.Interfaces.RepositoryInterfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Filters;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace SMedia.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
[Produces("application/json")]
public class UserController : ControllerBase
{
    private readonly IUserRepository _userRepository;
    private readonly ApplicationDbContext _context;

    public UserController(IUserRepository userRepository, ApplicationDbContext context)
    {
        _userRepository = userRepository;
        _context = context;
    }/// <summary>
    /// Search for users by name or username
    /// </summary>
    /// <param name="query">Search text</param>
    /// <param name="page">Page number (1-based)</param>
    /// <param name="pageSize">Number of results per page</param>
    /// <returns>List of matching users</returns>
    /// <response code="200">Returns the list of users matching the search criteria</response>
    /// <response code="400">If the search query is empty</response>
    [HttpGet("search")]
    //[AllowAnonymous] // Allow this endpoint without authentication for testing
    [SwaggerResponseExample(200, typeof(SMedia.SwaggerExamples.User.UserSearchResultExample))]
    public async Task<ActionResult<List<object>>> SearchUsers(
        [FromQuery] string query,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            return BadRequest("Search query cannot be empty");
        }        // For debugging, check if we have a user identity
        var userId = User.FindFirst("user_id")?.Value; // Use the correct claim name
        var userEmail = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;
        
        int skip = (page - 1) * pageSize;
        var users = await _userRepository.SearchUsers(query, skip, pageSize);

        // Map to simplified response
        var result = users.Select(u => new
        {
            userId = u.user_id,
            username = u.username,
            fullName = u.full_name,
            email = u.email,
            image = u.image
        }).ToList();
          // Include auth information for debugging
        var debugInfo = new
        {
            authenticated = User.Identity?.IsAuthenticated ?? false,
            userId = userId,
            userEmail = userEmail,
            authHeader = Request.Headers.ContainsKey("Authorization") ? 
                         "Present (first 20 chars): " + Request.Headers["Authorization"].ToString().Substring(0, Math.Min(Request.Headers["Authorization"].ToString().Length, 20)) :
                         "Not present"
        };
        
        // Add debug info to response headers using Append instead of Add
        Response.Headers.Append("X-Auth-Debug", System.Text.Json.JsonSerializer.Serialize(debugInfo));
        
        return Ok(result);    }
    
    /// <summary>
    /// Get detailed profile information for a specific user
    /// </summary>
    /// <param name="userId">The ID of the user to retrieve</param>
    /// <returns>The user's profile information</returns>
    /// <response code="200">Returns the user profile</response>
    /// <response code="404">If the user is not found</response>
    [HttpGet("{userId}")]
    [SwaggerResponseExample(200, typeof(SMedia.SwaggerExamples.User.UserProfileExample))]
    public async Task<ActionResult<object>> GetUserById(string userId)
    {
        var user = await _userRepository.GetUserById(userId);
        if (user == null)
        {
            return NotFound("User not found");
        }
          // Add debug info for authentication
        var authUserId = User.FindFirst("user_id")?.Value; // Use the correct claim name
        var authEmail = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;
        
        var debugInfo = new
        {
            authenticated = User.Identity?.IsAuthenticated ?? false,
            userId = authUserId,
            userEmail = authEmail,
            authHeader = Request.Headers.ContainsKey("Authorization") ? 
                         "Present (first 20 chars): " + Request.Headers["Authorization"].ToString().Substring(0, Math.Min(Request.Headers["Authorization"].ToString().Length, 20)) :
                         "Not present"
        };
        
        // Add debug info to response headers
        Response.Headers.Append("X-Auth-Debug", System.Text.Json.JsonSerializer.Serialize(debugInfo));        return Ok(new
        {
            userId = user.user_id,
            username = user.username,
            fullName = user.full_name,
            email = user.email,
            image = user.image,
            intro = user.intro,
            birthday = user.birthday,
            gender = user.gender,
            joinedAt = user.joined_at
        });
    }
      /// <summary>
    /// Get a list of random users for "Find Friends" feature
    /// </summary>
    /// <param name="count">Number of users to return (default: 10)</param>
    /// <returns>List of potential friends</returns>
    /// <response code="200">Returns list of users</response>
    /// <response code="401">Unauthorized</response>
    [HttpGet("suggestions")]
    public async Task<ActionResult<List<object>>> GetUserSuggestions([FromQuery] int count = 10)
    {
        // Get the current user's ID from claims - using the custom claim name "user_id"
        var currentUserId = User.FindFirst("user_id")?.Value;
        if (currentUserId == null)
        {
            return Unauthorized();
        }        // Debug header to track issues
        Response.Headers.Append("X-Auth-User-ID", currentUserId);
        
        // Limit the maximum number of users to 50 to prevent excessive requests
        count = Math.Min(count, 50);
        
        // Just get random users without checking if the current user exists
        var users = await _context.users
            .OrderBy(u => Guid.NewGuid()) // Random order
            .Take(count)
            .ToListAsync();
        
        // Add debug info for authentication
        var authEmail = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;
          var debugInfo = new
        {
            authenticated = User.Identity?.IsAuthenticated ?? false,
            userId = currentUserId,
            userEmail = authEmail,
            allClaims = User.Claims.Select(c => new { c.Type, c.Value }).ToDictionary(c => c.Type, c => c.Value),
            authHeader = Request.Headers.ContainsKey("Authorization") ? 
                         "Present (first 20 chars): " + Request.Headers["Authorization"].ToString().Substring(0, Math.Min(Request.Headers["Authorization"].ToString().Length, 20)) :
                         "Not present"
        };
        
        // Add debug info to response headers
        Response.Headers.Append("X-Auth-Debug", System.Text.Json.JsonSerializer.Serialize(debugInfo));
        
        // Map to simplified response
        var result = users.Select(u => new
        {
            userId = u.user_id,
            username = u.username,
            fullName = u.full_name,
            email = u.email,
            image = u.image
        }).ToList();
        
        return Ok(result);
    }
}
