// assetgaze-backend/src/Assetgaze.Backend/Features/Transactions/Linq2DbTransactionRepository.cs
// Regenerated to sync with ITransactionRepository.cs

using Assetgaze.Backend.Domain;
using LinqToDB;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System;

namespace Assetgaze.Backend.Features.Transactions
{
    public class Linq2DbTransactionRepository : ITransactionRepository
    {
        private readonly string _connectionString;

        public Linq2DbTransactionRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")
                                ?? throw new InvalidOperationException("DefaultConnection connection string is not configured.");
        }

        public async Task AddAsync(Transaction transaction)
        {
            await using var db = new AppDataConnection(_connectionString);
            await db.InsertAsync(transaction);
        }

        public async Task<Transaction?> GetByIdAsync(Guid id)
        {
            await using var db = new AppDataConnection(_connectionString);
            return await db.Transactions.FirstOrDefaultAsync(t => t.Id == id);
        }

        public async Task UpdateAsync(Transaction transaction)
        {
            await using var db = new AppDataConnection(_connectionString);
            await db.UpdateAsync(transaction);
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            await using var db = new AppDataConnection(_connectionString);
            var rowsAffected = await db.Transactions.Where(t => t.Id == id).DeleteAsync();
            return rowsAffected > 0; // Return true if at least one row was deleted
        }
        
        public async Task<IEnumerable<Transaction>> GetByAccountIdsAsync(IEnumerable<Guid> accountIds)
        {
            await using var db = new AppDataConnection(_connectionString);
            return await db.Transactions
                           .Where(t => accountIds.Contains(t.AccountId))
                           .ToListAsync();
        }
    }
}
