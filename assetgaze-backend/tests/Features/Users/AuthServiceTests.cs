// In: tests/Assetgaze.Tests/Features/Users/AuthServiceTests.cs

// Ensure these using directives are correct for your project structure
using Assetgaze.Backend.Features.Users; // For IAuthService, AuthService, IUserRepository, User
using Assetgaze.Backend.Features.Users.DTOs; // For LoginRequest, RegisterRequest, UserAccountPermission
using Microsoft.Extensions.Configuration;
using NUnit.Framework; // For TestFixture, SetUp, Test, Assert
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging; // Added for ILogger
using Microsoft.Extensions.Logging.Abstractions; // Added for NullLogger

namespace Assetgaze.Backend.Tests.Features.Users;

[TestFixture]
public class AuthServiceTests
{
    private FakeUserRepository _fakeUserRepo = null!;
    private IConfiguration _fakeConfiguration = null!;
    private IAuthService _authService = null!;
    private const string TestPassword = "Password123!";
    private string _hashedPassword = null!;
    // Removed Mock<ILogger<AuthService>> _mockLogger;

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
        
        // Pass NullLogger<AuthService>.Instance as the third argument to the AuthService constructor
        _authService = new AuthService(NullLogger<AuthService>.Instance, _fakeUserRepo, _fakeConfiguration);
    }

    // --- Registration Tests (from before) ---
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
        Assert.That(_fakeUserRepo.Users.Count, Is.EqualTo(1)); // Still only one user
    }

    // --- NEW LOGIN AND LOCKOUT TESTS ---

    [Test]
    public async Task LoginAsync_WithValidCredentials_ResetsFailedAttemptsAndReturnsToken()
    {
        // Arrange
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = "test@example.com",
            PasswordHash = _hashedPassword,
            FailedLoginAttempts = 3, // User has some previous failed attempts
            LoginCount = 5
        };
        _fakeUserRepo.Users.Add(user);

        var request = new LoginRequest { Email = "test@example.com", Password = TestPassword };

        // Act
        var token = await _authService.LoginAsync(request);

        // Assert
        Assert.That(token, Is.Not.Null);
        Assert.That(user.FailedLoginAttempts, Is.EqualTo(0)); // Should be reset
        Assert.That(user.LoginCount, Is.EqualTo(6)); // Should be incremented
        Assert.That(user.LastLoginDate, Is.Not.Null);
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
        Assert.That(user.FailedLoginAttempts, Is.EqualTo(3)); // Should be incremented
        Assert.That(user.LockoutEndDateUtc, Is.Null); // Should not be locked yet
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
        Assert.That(user.LockoutEndDateUtc, Is.GreaterThan(DateTime.UtcNow.AddMinutes(14))); // Check it's set for ~15 mins
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
            LockoutEndDateUtc = DateTime.UtcNow.AddMinutes(15) // Account is locked
        };
        _fakeUserRepo.Users.Add(user);
        var request = new LoginRequest { Email = "test@example.com", Password = TestPassword }; // Using correct password

        // Act
        var token = await _authService.LoginAsync(request);

        // Assert
        Assert.That(token, Is.Null); // Login should fail because the account is locked
    }
}
