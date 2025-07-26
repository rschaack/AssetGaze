// assetgaze-backend/src/Assetgaze.Backend/Features/Transactions/ITransactionService.cs

using Assetgaze.Backend.Features.Transactions.DTOs;

namespace Assetgaze.Backend.Features.Transactions
{
    public interface ITransactionService
    {
        // Simplified signatures: userId now passed directly, service handles account lookup
        Task<Transaction> SaveTransactionAsync(CreateTransactionRequest request, Guid userId, Guid accountId); // Added accountId directly
        Task<Transaction?> UpdateTransactionAsync(Guid id, UpdateTransactionRequest request, Guid userId);
        Task<bool> DeleteTransactionAsync(Guid id, Guid userId);
        Task<Transaction?> GetTransactionByIdAsync(Guid id, Guid userId); // Changed userId to Guid for consistency

        Task<IEnumerable<Transaction>> GetAllUserTransactionsAsync(string userId);
    }
}