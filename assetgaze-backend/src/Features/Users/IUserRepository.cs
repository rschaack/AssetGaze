// In: src/Assetgaze/Features/Users/IUserRepository.cs
namespace Assetgaze.Backend.Features.Users;

public interface IUserRepository
{
    Task<User?> GetByEmailAsync(string email);
    Task AddAsync(User user);
    Task UpdateAsync(User user);
    Task<List<Guid>> GetAccountIdsForUserAsync(Guid userId);
    Task AddUserAccountPermissionAsync(Guid userId, Guid accountId); 
}