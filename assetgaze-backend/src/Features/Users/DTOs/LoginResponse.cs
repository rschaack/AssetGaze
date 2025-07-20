namespace Assetgaze.Backend.Features.Users.DTOs
{
    public class LoginResponse
    {
        // The actual JWT (access token) will be sent in an HTTP-only cookie.
        // This property will now hold the Anti-Forgery Token (CSRF token)
        // that the frontend needs to send back on subsequent requests.
        public string CsrfToken { get; set; } = string.Empty;

        // You might include other non-sensitive user data here if needed for the frontend,
        // but avoid the JWT itself.
        // public string UserId { get; set; }
        // public string UserName { get; set; }
    }
}