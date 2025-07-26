using Assetgaze.Backend.Features.Users;
using Assetgaze.Backend.Features.Users.DTOs;
using Microsoft.Extensions.Configuration;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System.IdentityModel.Tokens.Jwt;

namespace Assetgaze.Backend.Tests.Features.Users
{
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
            
            _authService = new AuthService(
                NullLogger<AuthService>.Instance,
                _fakeUserRepo,
                _fakeConfiguration
            );
        }

        [Test]
        public async Task RegisterAsync_WithNewEmail_ShouldAddUserAndReturnTrue()
        {
            var request = new RegisterRequest { Email = "newuser@example.com", Password = TestPassword };
            var result = await _authService.RegisterAsync(request);
            Assert.That(result, Is.True);
            Assert.That(_fakeUserRepo.Users, Has.Count.EqualTo(1));
        }

        [Test]
        public async Task LoginAsync_UserWithPermissions_ReturnsTokenWithAccountPermissionClaims()
        {
            var userId = Guid.NewGuid();
            var accountId1 = Guid.NewGuid();
            var accountId2 = Guid.NewGuid();
            _fakeUserRepo.Users.Add(new User { Id = userId, Email = "perm_user@example.com", PasswordHash = _hashedPassword });
            await _fakeUserRepo.AddUserAccountPermissionAsync(userId, accountId1);
            await _fakeUserRepo.AddUserAccountPermissionAsync(userId, accountId2);
            var request = new LoginRequest { Email = "perm_user@example.com", Password = TestPassword };

            var token = await _authService.LoginAsync(request);

            Assert.That(token, Is.Not.Null);
            var handler = new JwtSecurityTokenHandler();
            var decodedToken = handler.ReadJwtToken(token);
            var permissionClaims = decodedToken.Claims
                .Where(c => c.Type == "account_permission")
                .Select(c => c.Value)
                .ToList();
            Assert.That(permissionClaims, Has.Count.EqualTo(2));
            Assert.That(permissionClaims, Contains.Item(accountId1.ToString()));
            Assert.That(permissionClaims, Contains.Item(accountId2.ToString()));
        }

        [Test]
        public async Task LoginAsync_WithFifthInvalidPassword_LocksAccount()
        {
            var user = new User { Id = Guid.NewGuid(), Email = "test@example.com", PasswordHash = _hashedPassword, FailedLoginAttempts = 4 };
            _fakeUserRepo.Users.Add(user);
            var request = new LoginRequest { Email = "test@example.com", Password = "wrong-password" };

            await _authService.LoginAsync(request);

            Assert.That(user.FailedLoginAttempts, Is.EqualTo(5));
            Assert.That(user.LockoutEndDateUtc, Is.Not.Null);
        }
    }
}