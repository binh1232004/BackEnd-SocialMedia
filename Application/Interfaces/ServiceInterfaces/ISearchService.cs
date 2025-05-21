using Application.DTOs;

namespace Application.Interfaces.ServiceInterfaces;

public interface ISearchService
{
    Task<List<UserDto>> SearchUsersAsync(string query, int skip, int take);
    Task<List<UserDto>> GetRandomUsersAsync(Guid currentUserId, int count);
    Task<List<UserDto>> AdvancedSearchUsersAsync(AdvancedSearchDto searchDto, int skip, int take);
}