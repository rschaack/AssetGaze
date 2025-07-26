// In: tests/Assetgaze.Tests/Features/Transactions/TransactionServiceTests.cs

using Assetgaze.Backend.Domain;
using Assetgaze.Backend.Features.Transactions;
using Assetgaze.Backend.Features.Transactions.DTOs;
using Assetgaze.Backend.Features.Users; // Added for FakeUserRepository and IUserRepository
using Microsoft.Extensions.Logging.Abstractions; // Added for NullLogger

namespace Assetgaze.Backend.Tests.Features.Transactions;

[TestFixture]
public class TransactionServiceTests
{
    private FakeTransactionRepository _fakeRepo = null!;
    private FakeUserRepository _fakeUserRepo = null!; // Added FakeUserRepository
    private ITransactionService _service = null!;

    [SetUp]
    public void SetUp()
    {
        _fakeRepo = new FakeTransactionRepository();
        _fakeUserRepo = new FakeUserRepository(); // Initialize FakeUserRepository
        
        // Create the service we are testing, injecting all required fake implementations and a NullLogger
        _service = new TransactionService(_fakeRepo, _fakeUserRepo, NullLogger<TransactionService>.Instance); // Updated constructor
    }

    [Test]
    public async Task SaveTransactionAsync_WithValidRequest_SavesTransactionToRepository()
    {
        // Arrange
        var loggedInUserId = Guid.NewGuid(); // A dummy user ID for the test
        var accountId = Guid.NewGuid();
        var request = new CreateTransactionRequest
        {
            TransactionType = TransactionType.Buy,
            BrokerId = Guid.NewGuid(),
            AccountId = accountId,
            TaxWrapper = TaxWrapper.ISA,
            ISIN = "US0378331005",
            TransactionDate = DateTime.UtcNow,
            Quantity = 10,
            NativePrice = 200.00m,
            LocalPrice = 200.00m,
            Consideration = 2000.00m
        };

        // Ensure the test user has permission to the account
        _fakeUserRepo.AddUserAccountPermissionAsync(loggedInUserId, accountId).Wait(); // Use .Wait() for async in SetUp

        var authorizedAccountIds = new List<Guid> { accountId };
        
        // Act
        var result = await _service.SaveTransactionAsync(request, loggedInUserId, authorizedAccountIds);

        // Assert
        Assert.That(_fakeRepo.Transactions.Count, Is.EqualTo(1));
        
        var savedTransaction = _fakeRepo.Transactions.First();

        Assert.That(savedTransaction, Is.Not.Null);
        Assert.That(savedTransaction.Id, Is.EqualTo(result.Id));
        Assert.That(savedTransaction.ISIN, Is.EqualTo(request.ISIN));
        Assert.That(savedTransaction.TransactionType, Is.EqualTo(request.TransactionType.ToString()));
        Assert.That(savedTransaction.TaxWrapper, Is.EqualTo(request.TaxWrapper.ToString()));
    }

    // --- NEW TEST: SaveTransactionAsync_WithUnauthorizedAccount_ThrowsUnauthorizedAccessException ---
    [Test]
    public void SaveTransactionAsync_WithUnauthorizedAccount_ThrowsUnauthorizedAccessException()
    {
        // Arrange
        var loggedInUserId = Guid.NewGuid();
        var accountId = Guid.NewGuid(); // Account the transaction is for
        var unauthorizedAccountId = Guid.NewGuid(); // An account the user *does not* have access to
        var request = new CreateTransactionRequest
        {
            TransactionType = TransactionType.Buy,
            BrokerId = Guid.NewGuid(),
            AccountId = accountId, // Request is for this account
            TaxWrapper = TaxWrapper.ISA,
            ISIN = "US0378331005",
            TransactionDate = DateTime.UtcNow,
            Quantity = 10, NativePrice = 10, LocalPrice = 10, Consideration = 10
        };

        // User is authorized for a *different* account, not the one in the request
        _fakeUserRepo.AddUserAccountPermissionAsync(loggedInUserId, unauthorizedAccountId).Wait();
        var authorizedAccountIds = new List<Guid> { unauthorizedAccountId };

        // Act & Assert
        Assert.ThrowsAsync<UnauthorizedAccessException>(async () =>
        {
            await _service.SaveTransactionAsync(request, loggedInUserId, authorizedAccountIds);
        });
        Assert.That(_fakeRepo.Transactions.Count, Is.EqualTo(0)); // No transaction should be saved
    }

    // --- NEW TEST: GetAllTransactionsForUserAsync_WithPermittedAccounts_ReturnsTransactions ---
    [Test]
    public async Task GetAllTransactionsForUserAsync_WithPermittedAccounts_ReturnsTransactions()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var accountId1 = Guid.NewGuid();
        var accountId2 = Guid.NewGuid();

        // Seed user permissions
        _fakeUserRepo.AddUserAccountPermissionAsync(userId, accountId1).Wait();
        _fakeUserRepo.AddUserAccountPermissionAsync(userId, accountId2).Wait();

        // Seed transactions for these accounts
        _fakeRepo.Transactions.Add(new Transaction { Id = Guid.NewGuid(), AccountId = accountId1, ISIN = "ISIN1" });
        _fakeRepo.Transactions.Add(new Transaction { Id = Guid.NewGuid(), AccountId = accountId2, ISIN = "ISIN2" });
        _fakeRepo.Transactions.Add(new Transaction { Id = Guid.NewGuid(), AccountId = Guid.NewGuid(), ISIN = "ISIN3" }); // Unauthorized account

        // Act
        var result = await _service.GetAllTransactionsForUserAsync(userId.ToString());

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Count(), Is.EqualTo(2)); // Only transactions for accountId1 and accountId2
        Assert.That(result.Any(t => t.ISIN == "ISIN1"), Is.True);
        Assert.That(result.Any(t => t.ISIN == "ISIN2"), Is.True);
        Assert.That(result.Any(t => t.ISIN == "ISIN3"), Is.False);
    }

    // --- NEW TEST: GetAllTransactionsForUserAsync_WithNoPermittedAccounts_ReturnsEmptyList ---
    [Test]
    public async Task GetAllTransactionsForUserAsync_WithNoPermittedAccounts_ReturnsEmptyList()
    {
        // Arrange
        var userId = Guid.NewGuid();
        // No permissions added for this user
        _fakeRepo.Transactions.Add(new Transaction { Id = Guid.NewGuid(), AccountId = Guid.NewGuid() }); // Some transactions exist

        // Act
        var result = await _service.GetAllTransactionsForUserAsync(userId.ToString());

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Count(), Is.EqualTo(0));
    }
}
