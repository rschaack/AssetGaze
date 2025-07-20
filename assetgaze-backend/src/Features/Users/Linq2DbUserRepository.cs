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
            return await db.Users.FirstOrDefaultAsync(u => u.Email.Equals(email, StringComparison.OrdinalIgnoreCase));
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

    // Ensure your AppDataConnection is set up to map to your database tables
    // Example (assuming it's in Assetgaze.Backend.Domain/AppDataConnection.cs):
    /*
    namespace Assetgaze.Backend.Domain
    {
        public class AppDataConnection : LinqToDB.Data.DataConnection
        {
            public AppDataConnection(string connectionString) : base(ProviderName.PostgreSQL, connectionString) { }

            public ITable<User> Users => GetTable<User>();
            public ITable<UserAccountPermission> UserAccountPermissions => GetTable<UserAccountPermission>();
            // Add other tables as needed
            public ITable<Broker> Brokers => GetTable<Broker>();
            public ITable<Account> Accounts => GetTable<Account>();
            public ITable<Transaction> Transactions => GetTable<Transaction>();
        }
    }
    */

    // Ensure your User, UserAccountPermission, Broker, Account, Transaction models are defined
    // and correctly mapped by LinqToDB.
    // Example User (from FakeUserRepository immersive, ensure it's consistent across your project):
    /*
    public class User
    {
        public Guid Id { get; set; }
        public string Email { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public int FailedLoginAttempts { get; set; }
        public DateTime? LockoutEndDateUtc { get; set; }
        public int LoginCount { get; set; }
        public DateTime? LastLoginDate { get; set; }
        public DateTime RegistrationDate { get; set; }
    }

    public class UserAccountPermission
    {
        public Guid UserId { get; set; }
        public Guid AccountId { get; set; }
    }
    */
}
