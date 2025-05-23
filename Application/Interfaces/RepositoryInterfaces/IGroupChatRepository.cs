namespace Application.Interfaces.RepositoryInterfaces;

public interface IGroupChatRepository
{
    Task<bool> IsUserInGroupAsync(Guid userId, Guid groupChatId);
}