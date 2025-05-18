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
        return await _context.Users.AnyAsync( u => u.Email == email);
    }
    public async Task<bool> IsUsernameExists(string username)
    {
        return await _context.Users.AnyAsync( u => u.Username == username);
    }

    public async Task<User> Create(User newUser)
    {
        _context.Users.Add(newUser);
        await _context.SaveChangesAsync();
        return newUser;
    }
    
    public async Task<User?> GetUserByEmail(string email)
    {
        return await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
    }
    
    public async Task<User?> GetUserById(Guid userId)
    {
        return await _context.Users.FirstOrDefaultAsync(u => u.UserId == userId);
    }
    public async Task<List<User>> SearchUsers(string query, int skip, int take)
    {
        // Convert query to lowercase for case-insensitive comparison
        var lowerQuery = query.ToLower();
        
        return await _context.Users
            .Where(u => u.Username.ToLower().Contains(lowerQuery) || 
                       (u.FullName != null && u.FullName.ToLower().Contains(lowerQuery)) || 
                       (u.Email != null && u.Email.ToLower().Contains(lowerQuery)))
            .OrderBy(u => u.Username)
            .Skip(skip)
            .Take(take)
            .ToListAsync();
    }
    
    public async Task<List<User>> GetRandomUsers(Guid currentUserId, int count)
    {
        // Get users except the current user, ordered by a random value
        return await _context.Users
            .Where(u => u.UserId != currentUserId)
            .OrderBy(u => Guid.NewGuid()) // This creates a randomized order
            .Take(count)
            .ToListAsync();
    }
}
