using Application.Interfaces.RepositoryInterfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Filters;

namespace SMedia.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
[Produces("application/json")]
public class UserController : ControllerBase
{
    private readonly IUserRepository _userRepository;

    public UserController(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }    /// <summary>
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
        }

        // For debugging, check if we have a user identity
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
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
        var authUserId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
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
        Response.Headers.Append("X-Auth-Debug", System.Text.Json.JsonSerializer.Serialize(debugInfo));

        return Ok(new
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
}
