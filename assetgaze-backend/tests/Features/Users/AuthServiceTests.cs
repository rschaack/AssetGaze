// In: tests/Assetgaze.Tests/Features/Users/AuthServiceTests.cs

using Assetgaze.Backend.Features.Users;
using Assetgaze.Backend.Features.Users.DTOs;
using Microsoft.Extensions.Configuration;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging.Abstractions;
using System.IdentityModel.Tokens.Jwt; // Added for JwtRegisteredClaimNames
using System.Security.Claims; // Added for ClaimTypes
using System.Text; // Added for Encoding
using Microsoft.IdentityModel.Tokens; // Added for SecurityTokenDescriptor

namespace Assetgaze.Backend.Tests.Features.Users;

[TestFixture]
public class AuthServiceTests
{
    private FakeUserRepository _fakeUserRepo = null!;
    private IConfiguration _fakeConfiguration = null!;
    private IAuthService _authService = null!;
    private const string TestPassword = "Password123!";
    private string _hashedPassword = null!;

    [SetUp]
    public void SetUp()
    {
        _fakeUserRepo = new FakeUserRepository();
        _hashedPassword = BCrypt.Net.BCrypt.HashPassword(TestPassword);

        var inMemorySettings = new Dictionary<string, string?>
        {
            {"Jwt:Key", "ThisIsMySuperSecretTestKeyThatIsVerySecure"},
            {"Jwt:Issuer", "https://test-issuer.com"},
            {"Jwt:Audience", "https://test-audience.com"}
        };
        _fakeConfiguration = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings)
            .Build();
        
        _authService = new AuthService(_fakeUserRepo, _fakeConfiguration, NullLogger<AuthService>.Instance);
    }

    [Test]
    public async Task RegisterAsync_WithNewEmail_ShouldAddUserAndReturnTrue()
    {
        var request = new RegisterRequest { Email = "newuser@example.com", Password = TestPassword };
        var result = await _authService.RegisterAsync(request);
        Assert.That(result, Is.True);
        Assert.That(_fakeUserRepo.Users.Count, Is.EqualTo(1));
        Assert.That(_fakeUserRepo.Users[0].Email, Is.EqualTo("newuser@example.com"));
    }

    [Test]
    public async Task RegisterAsync_WithExistingEmail_ShouldNotAddUserAndReturnFalse()
    {
        _fakeUserRepo.Users.Add(new User { Email = "existing@example.com", PasswordHash = _hashedPassword });
        var request = new RegisterRequest { Email = "existing@example.com", Password = TestPassword };
        var result = await _authService.RegisterAsync(request);
        Assert.That(result, Is.False);
        Assert.That(_fakeUserRepo.Users.Count, Is.EqualTo(1));
    }

    [Test]
    public async Task LoginAsync_WithValidCredentials_ResetsFailedAttemptsAndReturnsToken()
    {
        // Arrange
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = "test@example.com",
            PasswordHash = _hashedPassword,
            FailedLoginAttempts = 3,
            LoginCount = 5
        };
        _fakeUserRepo.Users.Add(user);

        var request = new LoginRequest { Email = "test@example.com", Password = TestPassword };

        // Act
        var token = await _authService.LoginAsync(request);

        // Assert
        Assert.That(token, Is.Not.Null);
        Assert.That(user.FailedLoginAttempts, Is.EqualTo(0));
        Assert.That(user.LoginCount, Is.EqualTo(6));
        Assert.That(user.LastLoginDate, Is.Not.Null);

        // Verify basic claims are present (no longer checking account_permission)
        var tokenHandler = new JwtSecurityTokenHandler();
        var jwtSecurityToken = tokenHandler.ReadJwtToken(token);
        Assert.That(jwtSecurityToken.Claims.Any(c => c.Type == JwtRegisteredClaimNames.Sub && c.Value == user.Id.ToString()), Is.True);
        Assert.That(jwtSecurityToken.Claims.Any(c => c.Type == JwtRegisteredClaimNames.Email && c.Value == user.Email), Is.True);
        Assert.That(jwtSecurityToken.Claims.Any(c => c.Type == JwtRegisteredClaimNames.Jti), Is.True);
        // Assert.That(jwtSecurityToken.Claims.Any(c => c.Type == "account_permission"), Is.False); // Optionally assert no account claims
    }

    [Test]
    public async Task LoginAsync_WithInvalidPassword_IncrementsFailedAttempts()
    {
        // Arrange
        var user = new User { Id = Guid.NewGuid(), Email = "test@example.com", PasswordHash = _hashedPassword, FailedLoginAttempts = 2 };
        _fakeUserRepo.Users.Add(user);
        var request = new LoginRequest { Email = "test@example.com", Password = "wrong-password" };

        // Act
        var token = await _authService.LoginAsync(request);

        // Assert
        Assert.That(token, Is.Null);
        Assert.That(user.FailedLoginAttempts, Is.EqualTo(3));
        Assert.That(user.LockoutEndDateUtc, Is.Null);
    }

    [Test]
    public async Task LoginAsync_WithFifthInvalidPassword_LocksAccount()
    {
        // Arrange
        var user = new User { Id = Guid.NewGuid(), Email = "test@example.com", PasswordHash = _hashedPassword, FailedLoginAttempts = 4 };
        _fakeUserRepo.Users.Add(user);
        var request = new LoginRequest { Email = "test@example.com", Password = "wrong-password" };

        // Act
        var token = await _authService.LoginAsync(request);

        // Assert
        Assert.That(token, Is.Null);
        Assert.That(user.FailedLoginAttempts, Is.EqualTo(5));
        Assert.That(user.LockoutEndDateUtc, Is.Not.Null);
        Assert.That(user.LockoutEndDateUtc, Is.GreaterThan(DateTime.UtcNow.AddMinutes(14)));
    }

    [Test]
    public async Task LoginAsync_WhenAccountIsLocked_ReturnsNull()
    {
        // Arrange
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = "test@example.com",
            PasswordHash = _hashedPassword,
            LockoutEndDateUtc = DateTime.UtcNow.AddMinutes(15)
        };
        _fakeUserRepo.Users.Add(user);
        var request = new LoginRequest { Email = "test@example.com", Password = TestPassword };

        // Act
        var token = await _authService.LoginAsync(request);

        // Assert
        Assert.That(token, Is.Null);
    }

    // Removed: GenerateTestJwtToken helper as it's no longer used by AuthServiceTests directly for token generation logic.
    // If you need a test JWT for controller tests, keep it in that test file.
}
