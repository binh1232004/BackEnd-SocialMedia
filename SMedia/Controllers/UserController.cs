using Application.Interfaces.RepositoryInterfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Filters;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Application.DTOs;
using Azure.Storage.Blobs;
using Microsoft.Extensions.Configuration;
using System.IO;

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
      /// <summary>
    /// Update a user's profile information, including optional image upload
    /// </summary>
    /// <param name="userId">ID of the user to update</param>
    /// <param name="userUpdateDto">The updated user information</param>
    /// <returns>The updated user profile</returns>
    /// <response code="200">Returns the updated user profile</response>
    /// <response code="400">If the user ID in the route doesn't match the authenticated user's ID or image upload fails</response>
    /// <response code="404">If the user is not found</response>
    [HttpPut("{userId}")]
    [Consumes("multipart/form-data")] // Allow form data for file upload
    public async Task<ActionResult<object>> UpdateUser(string userId, [FromForm] Application.DTOs.UserUpdateDto userUpdateDto)
    {
        // Get the current user's ID from claims
        var currentUserId = User.FindFirst("user_id")?.Value;
        if (currentUserId == null)
        {
            return Unauthorized();
        }
        
        // Only allow users to update their own profile (unless they have admin role)
        if (userId != currentUserId && !User.IsInRole("Admin"))
        {
            return Forbid("You can only update your own profile");
        }
        
        // Get the existing user
        var existingUser = await _userRepository.GetUserById(userId);
        if (existingUser == null)
        {
            return NotFound("User not found");
        }
          // Handle image upload if provided
        if (userUpdateDto.imageFile != null && userUpdateDto.imageFile.Length > 0)
        {
            // Get Azure Storage config from environment variables
            var connectionString = Environment.GetEnvironmentVariable("AZURE_STORAGE_CONNECTION_STRING");
            var containerName = Environment.GetEnvironmentVariable("AZURE_STORAGE_CONTAINER_NAME");
            
            if (string.IsNullOrEmpty(connectionString) || string.IsNullOrEmpty(containerName))
                return StatusCode(500, "Azure Storage configuration missing in environment variables.");
                
            // Upload to Azure Blob Storage
            try
            {
                var blobServiceClient = new BlobServiceClient(connectionString);
                var containerClient = blobServiceClient.GetBlobContainerClient(containerName);
                await containerClient.CreateIfNotExistsAsync();
                
                var fileName = $"user-images/{userId}_{Guid.NewGuid()}{Path.GetExtension(userUpdateDto.imageFile.FileName)}";
                var blobClient = containerClient.GetBlobClient(fileName);
                
                using (var stream = userUpdateDto.imageFile.OpenReadStream())
                {
                    await blobClient.UploadAsync(stream, overwrite: true);
                }
                
                // Set the image URL to the user
                existingUser.image = blobClient.Uri.ToString();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error uploading image: {ex.Message}");
            }
        }
        
        // Update other user properties
        if (userUpdateDto.fullName != null)
            existingUser.full_name = userUpdateDto.fullName;
            
        if (userUpdateDto.intro != null)
            existingUser.intro = userUpdateDto.intro;
            
        if (userUpdateDto.birthday.HasValue)
            existingUser.birthday = userUpdateDto.birthday;
            
        if (userUpdateDto.gender != null)
            existingUser.gender = userUpdateDto.gender;
            
        if (userUpdateDto.image != null) // This would be a direct URL provided in the request
            existingUser.image = userUpdateDto.image;
        
        // Save the updated user
        var updatedUser = await _userRepository.UpdateUser(userId, existingUser);
        
        return Ok(new
        {
            userId = updatedUser.user_id,
            username = updatedUser.username,
            fullName = updatedUser.full_name,
            email = updatedUser.email,
            image = updatedUser.image,
            intro = updatedUser.intro,
            birthday = updatedUser.birthday,
            gender = updatedUser.gender,
            joinedAt = updatedUser.joined_at
        });
    }
    
}
