// assetgaze-backend/src/Assetgaze.Backend/Features/Users/DTOs/LoginResponse.cs
// This DTO will now return the JWT directly.

namespace Assetgaze.Backend.Features.Users.DTOs
{
    public class LoginResponse
    {
        // The actual JWT (access token) will now be returned in the response body.
        public string Token { get; set; } = string.Empty;

        // You might include other non-sensitive user data here if needed for the frontend.
        // public string UserId { get; set; }
        // public string UserName { get; set; }
    }
}