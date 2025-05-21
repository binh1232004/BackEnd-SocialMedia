using Application.DTOs;
using Application.Interfaces.RepositoryInterfaces;
using Application.Interfaces.ServiceInterfaces;
using Mapster;

namespace Application.Services;

public class SearchService : ISearchService
{
    private readonly IUserRepository _userRepository;

    public SearchService(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<List<UserDto>> SearchUsersAsync(string query, int skip, int take)
    {
        var users = await _userRepository.SearchUsersAsync(query, skip, take);
        return users.Adapt<List<UserDto>>();
    }

    public async Task<List<UserDto>> GetRandomUsersAsync(Guid currentUserId, int count)
    {
        var users = await _userRepository.GetRandomUsersAsync(currentUserId, count);
        return users.Adapt<List<UserDto>>();
    }

    public async Task<List<UserDto>> AdvancedSearchUsersAsync(AdvancedSearchDto searchDto, int skip, int take)
    {
        var users = await _userRepository.AdvancedSearchUsersAsync(searchDto, skip, take);
        return users.Adapt<List<UserDto>>();
    }
}