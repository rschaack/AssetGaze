// assetgaze-backend/src/Assetgaze.Backend/Features/Users/AuthController.cs

using Assetgaze.Backend.Features.Users.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Assetgaze.Backend.Features.Users;

[ApiController]
[Route("api/[controller]")]
public class AuthController : Controller
{
    private readonly IAuthService _authService;
    // Removed: private readonly IAntiforgery _antiforgery; // No longer needed
    private readonly ILogger<AuthController> _logger;

    public AuthController(IAuthService authService, ILogger<AuthController> logger) // Removed IAntiforgery from constructor
    {
        _authService = authService;
        _logger = logger;
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

        // --- SIMPLIFIED TOKEN HANDLING ---
        _logger.LogInformation("Login: Generated JWT Token: {JwtToken}", token); // Log the token
        return Ok(new LoginResponse { Token = token });
        // --- END SIMPLIFIED TOKEN HANDLING ---
    }

    [HttpGet("status")]
    [Authorize] 
    public IActionResult GetAuthStatus()
    {
        _logger.LogInformation("Status: User is authenticated.");
        return Ok(new { isAuthenticated = true, message = "User is authenticated." });
    }

    [HttpPost("logout")]
    [Authorize] 
    public IActionResult Logout()
    {
        // --- SIMPLIFIED LOGOUT ---
        // No longer clearing specific cookies. Frontend will clear its token.
        _logger.LogInformation("Logout: User logged out.");
        return Ok(new { message = "Logged out successfully." });
        // --- END SIMPLIFIED LOGOUT ---
    }

    [HttpPost("protected-data")]
    [Authorize]
    public IActionResult PostProtectedData([FromBody] object data)
    {
        _logger.LogInformation("ProtectedData: Data received and authorized.");
        return Ok(new { message = "Data received and authorized!", receivedData = data });
    }
}
