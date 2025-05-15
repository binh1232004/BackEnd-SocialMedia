using Domain.Entities;

namespace Application.Interfaces.RepositoryInterfaces;

public interface IUserRepository
{
    Task<bool> IsEmailExists(string email);
    Task<user> Create(user newUser);
    Task<bool> IsUsernameExists(string username);
    Task<user?> GetUserByEmail(string email);
    Task<user?> GetUserById(string userId);
    Task<List<user>> SearchUsers(string query, int skip, int take);
}