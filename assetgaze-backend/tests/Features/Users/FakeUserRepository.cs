using Assetgaze.Backend.Features.Users;

namespace Assetgaze.Backend.Tests.Features.Users
{
    public class FakeUserRepository : IUserRepository
    {
        public readonly List<User> Users = new();
        public readonly List<UserAccountPermission> UserAccountPermissions = new();

        public Task<User?> GetByEmailAsync(string email)
        {
            var user = Users.FirstOrDefault(u => u.Email.Equals(email, StringComparison.OrdinalIgnoreCase));
            return Task.FromResult(user);
        }

        public Task AddAsync(User user)
        {
            Users.Add(user);
            return Task.CompletedTask;
        }

        public Task UpdateAsync(User user)
        {
            // The list holds the reference, so changes are automatically "persisted".
            return Task.CompletedTask;
        }

        public Task<List<Guid>> GetAccountIdsForUserAsync(Guid userId)
        {
            var accountIds = UserAccountPermissions
                .Where(p => p.UserId == userId)
                .Select(p => p.AccountId)
                .ToList();
            return Task.FromResult(accountIds);
        }

        public Task AddUserAccountPermissionAsync(Guid userId, Guid accountId)
        {
            UserAccountPermissions.Add(new UserAccountPermission { UserId = userId, AccountId = accountId });
            return Task.CompletedTask;
        }
    }
}