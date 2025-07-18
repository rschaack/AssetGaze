// In: src/Assetgaze/Features/Transactions/Services/TransactionSaveService.cs

using Assetgaze.Backend.Features.Accounts.DTOs;
using Assetgaze.Backend.Features.Users;

namespace Assetgaze.Backend.Features.Accounts;

public class AccountSaveService : IAccountSaveService
{
    private readonly IAccountRepository _AccountRepository;
    private readonly IUserRepository _userRepository;

    public AccountSaveService(IAccountRepository AccountRepository, IUserRepository userRepository)
    {
        _AccountRepository = AccountRepository;
        _userRepository = userRepository;
    }

    // The method signature doesn't need to change, but the mapping logic inside MUST be updated.
    public async Task<Account> SaveAccountAsync(CreateAccountRequest request, Guid loggedInUserId)
    {
        var newAccount = new Account
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
        };

        await _AccountRepository.AddAsync(newAccount);
        await _userRepository.AddUserAccountPermissionAsync(loggedInUserId, newAccount.Id);

        
        return newAccount;
    }
}