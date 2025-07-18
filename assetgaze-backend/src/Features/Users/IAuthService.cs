using Assetgaze.Backend.Features.Users.DTOs;

namespace Assetgaze.Backend.Features.Users;

public interface IAuthService
{
    Task<bool> RegisterAsync(RegisterRequest request);
    Task<string?> LoginAsync(LoginRequest request);
}