// In: src/Assetgaze/AppDataConnection.cs

using Assetgaze.Backend.Features.Accounts;
using Assetgaze.Backend.Features.Brokers;
using Assetgaze.Backend.Features.Transactions;
using Assetgaze.Backend.Features.Users;
using LinqToDB;
using LinqToDB.Data;

// Keep this using for other potential uses, but not directly for trace logging here

namespace Assetgaze.Backend;
    
public class AppDataConnection : DataConnection
{
    public AppDataConnection(string connectionString)
        : base(new DataOptions().UsePostgreSQL(connectionString))
    {
        // NEW: Simplified LinqToDB tracing to log SQL queries
        OnTraceConnection += info =>
        {
            if (info.TraceInfoStep == TraceInfoStep.BeforeExecute)
            {
                Console.WriteLine($"--- LinqToDB SQL Query ---");
                Console.WriteLine(info.SqlText); // This should definitely work
                Console.WriteLine($"Trace Step: {info.TraceInfoStep}"); // Log the trace step for context
                // Removed all code attempting to access info.Parameters or info.Data
                Console.WriteLine("--------------------------");
            }
        };
    }

    public ITable<Transaction> Transactions => this.GetTable<Transaction>();
    public ITable<User> Users => this.GetTable<User>();
    public ITable<Broker> Brokers => this.GetTable<Broker>();
    public ITable<Account> Accounts => this.GetTable<Account>();
    public ITable<UserAccountPermission> UserAccountPermissions => this.GetTable<UserAccountPermission>();
}