// assetgaze-backend/src/Assetgaze.Backend/Features/Transactions/TransactionService.cs

using Assetgaze.Backend.Domain;
using Assetgaze.Backend.Features.Transactions.DTOs;
using Assetgaze.Backend.Features.Users; // For IUserRepository
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System;

namespace Assetgaze.Backend.Features.Transactions
{
    public class TransactionService : ITransactionService
    {
        private readonly ITransactionRepository _transactionRepository;
        private readonly IUserRepository _userRepository; // Injected
        private readonly ILogger<TransactionService> _logger;

        public TransactionService(ITransactionRepository transactionRepository, IUserRepository userRepository, ILogger<TransactionService> logger)
        {
            _transactionRepository = transactionRepository;
            _userRepository = userRepository;
            _logger = logger;
        }

        public async Task<Transaction> SaveTransactionAsync(CreateTransactionRequest request, Guid userId, Guid accountId) // Simplified signature
        {
            var permittedAccountIds = await _userRepository.GetAccountIdsForUserAsync(userId);

            if (!permittedAccountIds.Contains(accountId)) // Check against the specific accountId from request
            {
                _logger.LogWarning("User {UserId} attempted to create transaction for unauthorized account {AccountId}", userId, accountId);
                throw new UnauthorizedAccessException("User does not have access to this account.");
            }

            var transaction = new Transaction
            {
                Id = Guid.NewGuid(),
                TransactionType = request.TransactionType.ToString(),
                BrokerId = request.BrokerId,
                AccountId = request.AccountId, // Use accountId from request
                TaxWrapper = request.TaxWrapper.ToString(),
                ISIN = request.ISIN,
                TransactionDate = request.TransactionDate,
                Quantity = request.Quantity,
                NativePrice = request.NativePrice,
                LocalPrice = request.LocalPrice,
                Consideration = request.Consideration,
                BrokerCharge = request.BrokerCharge,
                StampDuty = request.StampDuty,
                FxCharge = request.FxCharge,
                AccruedInterest = request.AccruedInterest,
                BrokerDealReference = request.BrokerDealReference,
            };

            await _transactionRepository.AddAsync(transaction);
            return transaction;
        }

        public async Task<Transaction?> UpdateTransactionAsync(Guid id, UpdateTransactionRequest request, Guid userId) // Simplified signature
        {
            var existingTransaction = await _transactionRepository.GetByIdAsync(id);
            if (existingTransaction == null) return null;

            var permittedAccountIds = await _userRepository.GetAccountIdsForUserAsync(userId);
            if (!permittedAccountIds.Contains(existingTransaction.AccountId))
            {
                _logger.LogWarning("User {UserId} attempted to update transaction {TransactionId} for unauthorized account {AccountId}", userId, id, existingTransaction.AccountId);
                throw new UnauthorizedAccessException("User does not have access to this account.");
            }
            // Also check if the request.AccountId (if changed) is authorized
            if (request.AccountId != existingTransaction.AccountId && !permittedAccountIds.Contains(request.AccountId))
            {
                _logger.LogWarning("User {UserId} attempted to update transaction {TransactionId} to unauthorized target account {AccountId}", userId, id, request.AccountId);
                throw new UnauthorizedAccessException("User does not have access to the target account for this update.");
            }


            // Update properties
            existingTransaction.TransactionType = request.TransactionType.ToString();
            existingTransaction.BrokerDealReference = request.BrokerDealReference;
            existingTransaction.BrokerId = request.BrokerId;
            existingTransaction.AccountId = request.AccountId;
            existingTransaction.TaxWrapper = request.TaxWrapper.ToString();
            existingTransaction.ISIN = request.ISIN;
            existingTransaction.TransactionDate = request.TransactionDate;
            existingTransaction.Quantity = request.Quantity;
            existingTransaction.NativePrice = request.NativePrice;
            existingTransaction.LocalPrice = request.LocalPrice;
            existingTransaction.Consideration = request.Consideration;
            existingTransaction.BrokerCharge = request.BrokerCharge;
            existingTransaction.StampDuty = request.StampDuty;
            existingTransaction.FxCharge = request.FxCharge;
            existingTransaction.AccruedInterest = request.AccruedInterest;

            await _transactionRepository.UpdateAsync(existingTransaction);
            return existingTransaction;
        }

        public async Task<bool> DeleteTransactionAsync(Guid id, Guid userId) // Simplified signature
        {
            var transaction = await _transactionRepository.GetByIdAsync(id);
            if (transaction == null) return false;

            var permittedAccountIds = await _userRepository.GetAccountIdsForUserAsync(userId);
            if (!permittedAccountIds.Contains(transaction.AccountId))
            {
                _logger.LogWarning("User {UserId} attempted to delete transaction {TransactionId} for unauthorized account {AccountId}", userId, id, transaction.AccountId);
                throw new UnauthorizedAccessException("User does not have access to delete this transaction.");
            }

            await _transactionRepository.DeleteAsync(id);
            return true;
        }

        public async Task<Transaction?> GetTransactionByIdAsync(Guid id, Guid userId) // Changed userId to Guid
        {
            var transaction = await _transactionRepository.GetByIdAsync(id);
            if (transaction == null) return null;

            var permittedAccountIds = await _userRepository.GetAccountIdsForUserAsync(userId);
            if (!permittedAccountIds.Contains(transaction.AccountId))
            {
                _logger.LogWarning("User {UserId} attempted to access transaction {TransactionId} for unauthorized account {AccountId}", userId, id, transaction.AccountId);
                return null; // Or throw UnauthorizedAccessException
            }

            return transaction;
        }

        public async Task<IEnumerable<Transaction>> GetAllUserTransactionsAsync(string userIdString) // userId is string from controller
        {
            var userId = Guid.Parse(userIdString); // Parse to Guid for repository lookup
            var permittedAccountIds = await _userRepository.GetAccountIdsForUserAsync(userId);

            if (!permittedAccountIds.Any())
            {
                _logger.LogInformation("No permitted accounts found for user {UserId}", userId);
                return Enumerable.Empty<Transaction>();
            }

            var transactions = await _transactionRepository.GetByAccountIdsAsync(permittedAccountIds);

            return transactions;
        }
    }
}
