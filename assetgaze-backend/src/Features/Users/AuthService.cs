// assetgaze-backend/src/Assetgaze.Backend/Features/Users/AuthService.cs

using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Assetgaze.Backend.Features.Users.DTOs;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using BCrypt.Net;
using Microsoft.Extensions.Logging;

namespace Assetgaze.Backend.Features.Users
{
    public class AuthService : IAuthService
    {
        private readonly ILogger<AuthService> _logger;
        private readonly IUserRepository _userRepository;
        private readonly IConfiguration _configuration;
        private const int MaxFailedLoginAttempts = 5;
        private const int LockoutDurationMinutes = 15;

        public AuthService(ILogger<AuthService> logger, IUserRepository userRepository, IConfiguration configuration)
        {
            _logger = logger;
            _userRepository = userRepository;
            _configuration = configuration;
        }

        public async Task<bool> RegisterAsync(RegisterRequest request)
        {
            var existingUser = await _userRepository.GetByEmailAsync(request.Email);
            if (existingUser != null) return false; 

            var hashedPassword = BCrypt.Net.BCrypt.HashPassword(request.Password);

            var newUser = new User
            {
                Id = Guid.NewGuid(),
                Email = request.Email,
                PasswordHash = hashedPassword,
                CreatedDate = DateTime.UtcNow,
                FailedLoginAttempts = 0,
                LoginCount = 0
            };

            await _userRepository.AddAsync(newUser);
            return true;
        }

        public async Task<string?> LoginAsync(LoginRequest request)
        {
            _logger.LogInformation("Login attempt for email: {Email}", request.Email);
            _logger.LogInformation("Password provided (first char): {PasswordFirstChar}", request.Password?.Length > 0 ? request.Password[0] : "N/A");
            _logger.LogInformation("Password length: {PasswordLength}", request.Password?.Length);

            var user = await _userRepository.GetByEmailAsync(request.Email);
            if (user == null) { return null; }
            
            if (user.LockoutEndDateUtc.HasValue && user.LockoutEndDateUtc.Value > DateTime.UtcNow)
            { return null; }
            
            var isPasswordValid = BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash);
            if (!isPasswordValid)
            {
                user.FailedLoginAttempts++;
                if (user.FailedLoginAttempts >= MaxFailedLoginAttempts)
                {
                    user.LockoutEndDateUtc = DateTime.UtcNow.AddMinutes(15);
                }
                await _userRepository.UpdateAsync(user);
                return null;
            }

            user.FailedLoginAttempts = 0;
            user.LockoutEndDateUtc = null;
            user.LoginCount++;
            user.LastLoginDate = DateTime.UtcNow;
            await _userRepository.UpdateAsync(user);

            var accountIds = await _userRepository.GetAccountIdsForUserAsync(user.Id);

            // 2. Pass the account IDs to the token generator
            var token = GenerateJwtToken(user, accountIds);
            return token;
        }
    
        // Modified: GenerateJwtToken no longer takes accountIds
        private string GenerateJwtToken(User user, IEnumerable<Guid> accountIds)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!);

            var claims = new List<Claim>
            {
                new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new(JwtRegisteredClaimNames.Email, user.Email),
                new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };
            
            claims.AddRange(accountIds.Select(accountId => new Claim("account_permission", accountId.ToString())));

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddHours(1),
                Issuer = _configuration["Jwt:Issuer"],
                Audience = _configuration["Jwt:Audience"],
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }
}
