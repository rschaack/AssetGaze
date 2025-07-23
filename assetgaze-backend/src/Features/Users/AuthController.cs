// assetgaze-backend/src/Assetgaze.Backend/Features/Users/AuthController.cs
// (Integrate these new methods into your existing AuthController)

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using Assetgaze.Backend.Features.Users.DTOs;
using Microsoft.Extensions.Logging; // Added for ILogger

namespace Assetgaze.Backend.Features.Users;

[ApiController]
[Route("api/[controller]")]
public class AuthController : Controller
{
    private readonly IAuthService _authService;
    private readonly IAntiforgery _antiforgery;
    private readonly ILogger<AuthController> _logger; // Added ILogger

    public AuthController(IAuthService authService, IAntiforgery antiforgery, ILogger<AuthController> logger) // Inject ILogger
    {
        _authService = authService;
        _antiforgery = antiforgery;
        _logger = logger; // Assign logger
    }
    
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        var wasRegistrationSuccessful = await _authService.RegisterAsync(request);

        if (!wasRegistrationSuccessful)
        {
            return BadRequest("Registration failed. An account with this email may already exist.");
        }

        return Ok("User registered successfully.");
    }
    
    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var token = await _authService.LoginAsync(request);

        if (token is null)
        {
            return Unauthorized("Invalid email or password.");
        }

        Response.Cookies.Append("access_token", token, new CookieOptions
        {
            HttpOnly = true,
            Secure = true, // Ensure this is true for HTTPS
            SameSite = SameSiteMode.Lax,
            Expires = DateTime.UtcNow.AddMinutes(30) // Access token expiration
        });

        var antiforgeryToken = _antiforgery.GetAndStoreTokens(HttpContext).RequestToken;
        
        // --- LOGGING ---
        _logger.LogInformation("Login: Generated Anti-Forgery Token: {AntiforgeryToken}", antiforgeryToken);
        // --- END LOGGING ---

        Response.Cookies.Append("XSRF-TOKEN", antiforgeryToken!, new CookieOptions
        {
            HttpOnly = false,
            Secure = true,
            SameSite = SameSiteMode.Lax,
            Expires = DateTime.UtcNow.AddMinutes(30)
        });
        
        return Ok(new LoginResponse { CsrfToken = antiforgeryToken! });
    }

    [HttpGet("status")]
    [Authorize]
    public IActionResult GetAuthStatus()
    {
        return Ok(new { isAuthenticated = true, message = "User is authenticated." });
    }

    [HttpPost("logout")]
    [Authorize]
    public IActionResult Logout()
    {
        Response.Cookies.Append("access_token", "", new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Lax,
            Expires = DateTime.UtcNow.AddDays(-1)
        });

        Response.Cookies.Append("XSRF-TOKEN", "", new CookieOptions
        {
            HttpOnly = false,
            Secure = true,
            SameSite = SameSiteMode.Lax,
            Expires = DateTime.UtcNow.AddDays(-1)
        });

        return Ok(new { message = "Logged out successfully." });
    }

    [HttpPost("protected-data")]
    [Authorize]
    [ValidateAntiForgeryToken]
    public IActionResult PostProtectedData([FromBody] object data)
    {
        // --- LOGGING ---
        var requestToken = HttpContext.Request.Headers["X-XSRF-TOKEN"].FirstOrDefault();
        var cookieToken = HttpContext.Request.Cookies["XSRF-TOKEN"];
        _logger.LogInformation("ProtectedData: Request Header X-XSRF-TOKEN: {RequestToken}", requestToken);
        _logger.LogInformation("ProtectedData: Request Cookie XSRF-TOKEN: {CookieToken}", cookieToken);
        // --- END LOGGING ---

        return Ok(new { message = "Data received and authorized with CSRF!", receivedData = data });
    }
}
