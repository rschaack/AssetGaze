using Assetgaze.Backend.Features.Accounts.DTOs;

namespace Assetgaze.Backend.Features.Accounts;

public interface IAccountSaveService
{
    Task<Account> SaveAccountAsync(CreateAccountRequest request, Guid loggedInUserId);
}