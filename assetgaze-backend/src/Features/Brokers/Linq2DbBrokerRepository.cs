using LinqToDB;

namespace Assetgaze.Backend.Features.Brokers;

public class Linq2DbBrokerRepository : IBrokerRepository
{
    private readonly string _connectionString;

    // We now inject IConfiguration to get the connection string
    public Linq2DbBrokerRepository(IConfiguration configuration)
    {
        // We get the connection string once and store it
        _connectionString = configuration.GetConnectionString("DefaultConnection") 
                            ?? throw new InvalidOperationException("DefaultConnection string is not configured.");
    }

    public async Task AddAsync(Broker broker)
    {
        // Create a new connection for this specific operation
        await using var db = new AppDataConnection(_connectionString);
        await db.InsertAsync(broker);
    }

    public async Task<Broker?> GetByIdAsync(Guid id)
    {
        // Create a new connection for this specific operation
        await using var db = new AppDataConnection(_connectionString);
        return await db.Brokers
            .Where(t => t.Id == id)
            .FirstOrDefaultAsync();
    }
    
    public async Task<List<Broker?>> GetAllAsync()
    {
        // Create a new connection for this specific operation
        await using var db = new AppDataConnection(_connectionString);
        return await db.Brokers.ToListAsync();
    }
}