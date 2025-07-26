// assetgaze-backend/src/Assetgaze.Backend/Features/Users/Linq2DbUserRepository.cs

// Assuming your AppDataConnection and User/Account models are here
// Assuming User DTO is here
using LinqToDB;

namespace Assetgaze.Backend.Features.Users
{
    public class Linq2DbUserRepository : IUserRepository
    {
        private readonly string _connectionString;

        public Linq2DbUserRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")
                                ?? throw new ArgumentNullException("DefaultConnection connection string is not configured.");
        }

        // Implementation of IUserRepository methods using LinqToDB
        public async Task<User?> GetByEmailAsync(string email)
        {
            await using var db = new AppDataConnection(_connectionString);
            return await db.Users
                .Where(u => u.Email.ToLower() == email.ToLower()) // Corrected comparison
                .FirstOrDefaultAsync();
        }

        public async Task AddAsync(User user)
        {
            await using var db = new AppDataConnection(_connectionString);
            await db.InsertAsync(user);
        }

        public async Task UpdateAsync(User user)
        {
            await using var db = new AppDataConnection(_connectionString);
            // LinqToDB's UpdateAsync requires the entity to be attached or explicitly specified
            await db.UpdateAsync(user);
        }

        public async Task<List<Guid>> GetAccountIdsForUserAsync(Guid userId)
        {
            await using var db = new AppDataConnection(_connectionString);
            return await db.UserAccountPermissions
                           .Where(p => p.UserId == userId)
                           .Select(p => p.AccountId)
                           .ToListAsync();
        }

        public async Task AddUserAccountPermissionAsync(Guid userId, Guid accountId)
        {
            await using var db = new AppDataConnection(_connectionString);
            var permission = new UserAccountPermission { UserId = userId, AccountId = accountId };
            await db.InsertAsync(permission);
        }
    }
}
