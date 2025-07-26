// assetgaze-backend/src/Assetgaze.Backend/Features/Transactions/TransactionsController.cs

using System.Security.Claims;
using Assetgaze.Backend.Features.Transactions.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Assetgaze.Backend.Domain;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System;
using Microsoft.Extensions.Logging;

namespace Assetgaze.Backend.Features.Transactions;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class TransactionsController : ControllerBase
{
    private readonly ITransactionService _transactionService;
    private readonly ITransactionRepository _transactionRepository;
    private readonly ILogger<TransactionsController> _logger;

    public TransactionsController(ITransactionService transactionService, ITransactionRepository transactionRepository, ILogger<TransactionsController> logger)
    {
        _transactionService = transactionService;
        _transactionRepository = transactionRepository;
        _logger = logger;
    }

    [HttpPost]
    public async Task<IActionResult> PostTransaction([FromBody] CreateTransactionRequest request)
    {
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userIdString is null)
        {
            _logger.LogWarning("PostTransaction: User ID not found in claims.");
            return Unauthorized();
        }
        var userId = Guid.Parse(userIdString);

        // Removed: Getting account IDs from JWT claims. Now service will do DB lookup.
        // var authorizedAccountIds = User.FindAll("account_permission").Select(c => Guid.Parse(c.Value)).ToList();

        try
        {
            // Pass userId directly. Service will perform DB lookup for authorized accounts.
            var createdTransaction = await _transactionService.SaveTransactionAsync(request, userId, request.AccountId); // Simplified signature
            return CreatedAtAction(nameof(GetTransactionById), new { id = createdTransaction.Id }, createdTransaction);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning("PostTransaction: Unauthorized access attempt by {UserId}: {Message}", userId, ex.Message);
            return StatusCode(403, new { message = ex.Message });
        }
    }
    
    [HttpGet("{id}")]
    public async Task<IActionResult> GetTransactionById(Guid id)
    {
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userIdString is null)
        {
            _logger.LogWarning("GetTransactionById: User ID not found in claims.");
            return Unauthorized();
        }
        var loggedInUserId = Guid.Parse(userIdString);

        // Removed: Getting account IDs from JWT claims. Now service will do DB lookup.
        // var authorizedAccountIds = User.FindAll("account_permission").Select(c => Guid.Parse(c.Value)).ToList();

        // Service now handles authorization check internally
        var transaction = await _transactionService.GetTransactionByIdAsync(id, loggedInUserId); // Pass userId directly

        if (transaction is null)
        {
            _logger.LogInformation("GetTransactionById: Transaction {TransactionId} not found or unauthorized.", id);
            return NotFound(); // This could be NotFound or Forbid depending on service logic
        }

        return Ok(transaction);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> PutTransaction(Guid id, [FromBody] UpdateTransactionRequest request)
    {
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userIdString is null)
        {
            _logger.LogWarning("PutTransaction: User ID not found in claims.");
            return Unauthorized();
        }
        var userId = Guid.Parse(userIdString);

        // Removed: Getting account IDs from JWT claims. Now service will do DB lookup.
        // var authorizedAccountIds = User.FindAll("account_permission").Select(c => Guid.Parse(c.Value)).ToList();

        try
        {
            // Pass userId directly. Service will perform DB lookup for authorized accounts.
            var updatedTransaction = await _transactionService.UpdateTransactionAsync(id, request, userId); // Simplified signature

            if (updatedTransaction is null)
            {
                _logger.LogInformation("PutTransaction: Transaction {TransactionId} not found for update or unauthorized.", id);
                return NotFound();
            }
            return Ok(updatedTransaction);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning("PutTransaction: Unauthorized access attempt by {UserId}: {Message}", userId, ex.Message);
            return StatusCode(403, new { message = ex.Message });
        }
    }
    
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteTransaction(Guid id)
    {
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userIdString is null)
        {
            _logger.LogWarning("DeleteTransaction: User ID not found in claims.");
            return Unauthorized();
        }

        var userId = Guid.Parse(userIdString);

        // Removed: Getting account IDs from JWT claims. Now service will do DB lookup.
        // var authorizedAccountIds = User.FindAll("account_permission").Select(c => Guid.Parse(c.Value)).ToList();

        try
        {
            // Pass userId directly. Service will perform DB lookup for authorized accounts.
            var wasDeleted = await _transactionService.DeleteTransactionAsync(id, userId); // Simplified signature

            if (!wasDeleted)
            {
                _logger.LogInformation("DeleteTransaction: Transaction {TransactionId} not found for deletion or unauthorized.", id);
                return NotFound();
            }
            return NoContent();
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning("DeleteTransaction: Unauthorized access attempt by {UserId}: {Message}", userId, ex.Message);
            return StatusCode(403, new { message = ex.Message });
        }
    }

    [HttpGet("user")]
    public async Task<ActionResult<IEnumerable<Transaction>>> GetAllUserTransactions()
    {
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userIdString))
        {
            _logger.LogWarning("GetAllUserTransactions: User ID not found in claims.");
            return Unauthorized("User ID not found.");
        }

        _logger.LogInformation("GetAllUserTransactions: Fetching all transactions for user {UserId}", userIdString);
        
        // Service will perform DB lookup for authorized accounts.
        var transactions = await _transactionService.GetAllUserTransactionsAsync(userIdString);

        if (transactions == null || !transactions.Any())
        {
            _logger.LogInformation("GetAllUserTransactions: No transactions found for user {UserId}", userIdString);
            return NotFound("No transactions found for this user.");
        }

        return Ok(transactions);
    }
}
