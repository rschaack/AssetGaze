using Assetgaze.Backend.Domain;
using Assetgaze.Backend.Features.Transactions;
using Assetgaze.Backend.Features.Transactions.DTOs;
using Assetgaze.Backend.Features.Users;
using Microsoft.Extensions.Logging.Abstractions;
using NUnit.Framework;
using System;
using System.Linq;
using System.Threading.Tasks;
using Assetgaze.Backend.Tests.Features.Users;

namespace Assetgaze.Backend.Tests.Features.Transactions;

[TestFixture]
public class TransactionServiceTests
{
    private FakeTransactionRepository _fakeRepo = null!;
    private FakeUserRepository _fakeUserRepo = null!;
    private ITransactionService _service = null!;

    [SetUp]
    public void SetUp()
    {
        _fakeRepo = new FakeTransactionRepository();
        _fakeUserRepo = new FakeUserRepository();
        _service = new TransactionService(_fakeRepo, _fakeUserRepo, NullLogger<TransactionService>.Instance);
    }

    [Test]
    public async Task SaveTransactionAsync_WhenUserIsAuthorized_SavesTransaction()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var accountId = Guid.NewGuid();
        await _fakeUserRepo.AddUserAccountPermissionAsync(userId, accountId);
        
        var request = new CreateTransactionRequest
        {
            AccountId = accountId,
            TransactionType = TransactionType.Buy,
            BrokerId = Guid.NewGuid(),
            TaxWrapper = TaxWrapper.ISA,
            ISIN = "US0378331005",
            TransactionDate = DateTime.UtcNow,
            Quantity = 10, NativePrice = 10, LocalPrice = 10, Consideration = 10
        };
        
        // ✅ **FIX:** The method call now includes the required 'accountId' parameter.
        var result = await _service.CreateTransactionAsync(request, userId, accountId);

        // Assert
        Assert.That(_fakeRepo.Transactions, Has.Count.EqualTo(1));
        var savedTransaction = _fakeRepo.Transactions.First();
        Assert.That(savedTransaction.Id, Is.EqualTo(result.Id));
        Assert.That(savedTransaction.AccountId, Is.EqualTo(accountId));
    }

    [Test]
    public void SaveTransactionAsync_WhenUserIsNotAuthorized_ThrowsUnauthorizedAccessException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var authorizedAccountId = Guid.NewGuid();
        var unauthorizedAccountId = Guid.NewGuid(); // The account in the request

        // User has permission, but for a different account
        _fakeUserRepo.UserAccountPermissions.Add(new UserAccountPermission { UserId = userId, AccountId = authorizedAccountId });
        
        var request = new CreateTransactionRequest
        {
            AccountId = unauthorizedAccountId, // Request is for the unauthorized account
            ISIN = "GB00BH4HKS39", TransactionType = TransactionType.Sell, BrokerId = Guid.NewGuid(), TaxWrapper = TaxWrapper.SIPP, TransactionDate = DateTime.UtcNow, Quantity = 5, NativePrice = 5, LocalPrice = 5, Consideration = 25
        };

        // ✅ **FIX:** Pass the accountId from the request to the method call.
        Assert.ThrowsAsync<UnauthorizedAccessException>(async () => await _service.CreateTransactionAsync(request, userId, unauthorizedAccountId));
        
        Assert.That(_fakeRepo.Transactions, Is.Empty);
    }
}