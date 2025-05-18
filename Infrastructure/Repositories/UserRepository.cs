using Application.Interfaces.RepositoryInterfaces;
using Domain.Entities;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class UserRepository : IUserRepository
{
    private readonly ApplicationDbContext _context;

    public UserRepository(ApplicationDbContext context)
    {
        _context = context;
    }
    public async Task<bool> IsEmailExists(string email)
    {
        return await _context.users.AnyAsync( u => u.email == email);
    }
    public async Task<bool> IsUsernameExists(string username)
    {
        return await _context.users.AnyAsync( u => u.username == username);
    }

    public async Task<user> Create(user newUser)
    {
        _context.users.Add(newUser);
        await _context.SaveChangesAsync();
        return newUser;
    }
    
    public async Task<user?> GetUserByEmail(string email)
    {
        return await _context.users.FirstOrDefaultAsync(u => u.email == email);
    }
    
    public async Task<user?> GetUserById(string userId)
    {
        return await _context.users.FirstOrDefaultAsync(u => u.user_id == userId);
    }
    public async Task<List<user>> SearchUsers(string query, int skip, int take)
    {
        // Convert query to lowercase for case-insensitive comparison
        var lowerQuery = query.ToLower();
        
        return await _context.users
            .Where(u => u.username.ToLower().Contains(lowerQuery) || 
                       (u.full_name != null && u.full_name.ToLower().Contains(lowerQuery)) || 
                       (u.email != null && u.email.ToLower().Contains(lowerQuery)))
            .OrderBy(u => u.username)
            .Skip(skip)
            .Take(take)
            .ToListAsync();
    }
      public async Task<List<user>> GetRandomUsers(string currentUserId, int count)
    {
        // Get users except the current user, ordered by a random value
        return await _context.users
            .Where(u => u.user_id != currentUserId)
            .OrderBy(u => Guid.NewGuid()) // This creates a randomized order
            .Take(count)
            .ToListAsync();
    }
    
    public async Task<user?> UpdateUser(string userId, user updatedUser)
    {
        var existingUser = await _context.users.FirstOrDefaultAsync(u => u.user_id == userId);
        if (existingUser == null)
        {
            return null;
        }

        // Update only the fields that are allowed to be modified
        existingUser.full_name = updatedUser.full_name;
        existingUser.intro = updatedUser.intro;
        existingUser.birthday = updatedUser.birthday;
        existingUser.gender = updatedUser.gender;
        existingUser.image = updatedUser.image;
        
        _context.users.Update(existingUser);
        await _context.SaveChangesAsync();
        
        return existingUser;
    }
}
