using Assetgaze.Backend.Features.Users.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace Assetgaze.Backend.Features.Users;

[ApiController]
[Route("api/[controller]")] // This will make the URL /api/auth
public class AuthController : Controller
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
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
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var token = await _authService.LoginAsync(request);

        if (token is null)
        {
            return Unauthorized("Invalid email or password.");
        }

        return Ok(new { Token = token });
    }
}