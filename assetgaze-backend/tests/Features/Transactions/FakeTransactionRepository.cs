using Assetgaze.Backend.Features.Transactions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Assetgaze.Backend.Tests.Features.Transactions
{
    public class FakeTransactionRepository : ITransactionRepository
    {
        public readonly List<Transaction> Transactions = new();

        public Task AddAsync(Transaction transaction)
        {
            Transactions.Add(transaction);
            return Task.CompletedTask;
        }

        public Task<Transaction?> GetByIdAsync(Guid id)
        {
            var transaction = Transactions.FirstOrDefault(t => t.Id == id);
            return Task.FromResult(transaction);
        }
        
        // ... UpdateAsync and DeleteAsync methods remain the same ...
        public Task UpdateAsync(Transaction transaction)
        {
            var existing = Transactions.FirstOrDefault(t => t.Id == transaction.Id);
            if (existing != null) { /* ... property updates ... */ }
            return Task.CompletedTask;
        }

        public Task<bool> DeleteAsync(Guid id)
        {
            var removedCount = Transactions.RemoveAll(t => t.Id == id);
            return Task.FromResult(removedCount > 0);
        }


        // ✅ NEW: Implement the new method for the fake repository.
        public Task<IEnumerable<Transaction>> GetByAccountIdsAsync(IEnumerable<Guid> accountIds)
        {
            // Use LINQ to filter the in-memory list.
            var results = Transactions.Where(t => accountIds.Contains(t.AccountId));
            return Task.FromResult(results);
        }
    }
}