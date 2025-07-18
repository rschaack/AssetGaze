// In: src/Assetgaze/Features/Transactions/ITransactionService.cs

using Assetgaze.Backend.Features.Transactions.DTOs;

namespace Assetgaze.Backend.Features.Transactions;

public interface ITransactionService
{
    Task<Transaction> SaveTransactionAsync(CreateTransactionRequest request, Guid loggedInUserId, List<Guid> authorizedAccountIds);
    Task<Transaction?> UpdateTransactionAsync(Guid transactionId, UpdateTransactionRequest request, Guid loggedInUserId, List<Guid> authorizedAccountIds);
    Task<bool> DeleteTransactionAsync(Guid transactionId, Guid loggedInUserId, List<Guid> authorizedAccountIds);
}