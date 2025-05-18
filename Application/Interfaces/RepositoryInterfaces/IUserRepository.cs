using Domain.Entities;

namespace Application.Interfaces.RepositoryInterfaces;

public interface IUserRepository
{
    Task<bool> IsEmailExists(string email);
    Task<User> Create(User newUser);
    Task<bool> IsUsernameExists(string username);
    Task<User?> GetUserByEmail(string email);
    Task<User?> GetUserById(Guid userId);
    Task<List<User>> SearchUsers(string query, int skip, int take);
    Task<List<User>> GetRandomUsers(Guid currentUserId, int count);
}