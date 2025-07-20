using Assetgaze.Backend.Features.Users.DTOs;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Assetgaze.Backend.Features.Users;

[ApiController]
[Route("api/[controller]")] // This will make the URL /api/auth
public class AuthController : Controller
{
    private readonly IAuthService _authService;
    private readonly IAntiforgery _antiforgery;

    public AuthController(IAuthService authService, IAntiforgery antiforgery)
    {
        _authService = authService;
        _antiforgery = antiforgery;
    }
    
    [HttpPost("register")] // This maps to the URL: POST /api/auth/register
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        var wasRegistrationSuccessful = await _authService.RegisterAsync(request);

        if (!wasRegistrationSuccessful)
        {
            // We use a general message to avoid confirming whether an email is already registered.
            return BadRequest("Registration failed. An account with this email may already exist.");
        }

        return Ok("User registered successfully.");
    }
    
    [HttpPost("login")] // This maps to the URL: POST /api/auth/login
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var token = await _authService.LoginAsync(request); // This should return the JWT string

        if (token is null)
        {
            return Unauthorized("Invalid email or password.");
        }

        // --- START: Secure Token Handling Changes ---

        // 1. Set the JWT (Access Token) in an HTTP-only, Secure, SameSite=Lax cookie
        Response.Cookies.Append("access_token", token, new CookieOptions
        {
            HttpOnly = true,       // Prevents JavaScript from accessing the cookie (XSS protection)
            Secure = true,         // Only send over HTTPS
            SameSite = SameSiteMode.Lax, // Recommended for CSRF mitigation, allows GET from other sites
            // but requires POST to be same-site or have CSRF token
            Expires = DateTime.UtcNow.AddMinutes(30) // Set appropriate expiration for your access token
        });

        // 2. Generate and return an Anti-Forgery Token (CSRF Token) in the response body
        // The frontend will read this and send it back in a custom header on subsequent POST/PUT/DELETE requests.
        var tokens = _antiforgery.GetAndStoreTokens(HttpContext);
        
        // Return the CSRF token in the response body
        return Ok(new LoginResponse { CsrfToken = tokens.RequestToken! });

        // --- END: Secure Token Handling Changes ---
    }
}