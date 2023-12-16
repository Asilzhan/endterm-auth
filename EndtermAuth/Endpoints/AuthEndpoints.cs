using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using EndtermAuth.Data;
using EndtermAuth.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace EndtermAuth.Endpoints;

public static class AuthEndpoints
{
    public static RouteGroupBuilder MapAuthEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("auth")
            .WithTags("Authentication")
            .AllowAnonymous();
        
        group.MapPost("/register", async (UserRegistrationRequest request, AppDbContext db) =>
        {
            var user = await db.Users.FirstOrDefaultAsync(u => u.Username == request.Username);
            if (user is not null) return Results.BadRequest("User already exists!");

            user = new User
            {
                Id = 0,
                Username = request.Username,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
                DateOfBirth = request.DateOfBirth,
                Role = request.Role
            };
            
            db.Users.Add(user);
            await db.SaveChangesAsync();
            return Results.Ok();
        });
        
        group.MapPost("/login", async (LoginRequest loginRequest, AppDbContext db, IOptions<JwtSettings> jwtSettings) =>
        {
            var user = await db.Users.FirstOrDefaultAsync(u => u.Username == loginRequest.Username);
            if (user == null || !BCrypt.Net.BCrypt.Verify(loginRequest.Password, user.PasswordHash))
            {
                return Results.Unauthorized();
            }

            var token = GenerateJwtToken(user, jwtSettings.Value.SecretKey);
            var refreshToken = GenerateRefreshToken();
            var userLogin = await db.UserLogins.FirstOrDefaultAsync(ul => ul.UserId == user.Id);
            if (userLogin == null)
            {
                userLogin = new UserLogin
                {
                    UserId = user.Id,
                    User = user
                };
                db.UserLogins.Add(userLogin);
            }
            userLogin.RefreshToken = refreshToken;
            userLogin.LastLogin = DateTime.UtcNow;
            
            await db.SaveChangesAsync();
            
            return Results.Json(new { token, refreshToken });
        });
        
        group.MapPost("/refresh-token", async (string token, string refreshToken, AppDbContext db, IOptions<JwtSettings> jwtSettings) =>
        {
            var principal = GetPrincipalFromExpiredToken(token, jwtSettings.Value.SecretKey);
            if (principal.Identity is null) return Results.Unauthorized();
            
            var username = principal.Identity.Name;
            var user = await db.Users.SingleOrDefaultAsync(u => u.Username == username);
            if (user is null) return Results.Unauthorized();
            
            var userLogin = await db.UserLogins.SingleOrDefaultAsync(ul => ul.UserId == user.Id && ul.RefreshToken == refreshToken);

            if (userLogin == null || userLogin.RefreshTokenExpiryTime <= DateTime.UtcNow)
            {
                return Results.Unauthorized();
            }

            var newJwtToken = GenerateJwtToken(user, jwtSettings.Value.SecretKey);
            var newRefreshToken = GenerateRefreshToken();
            userLogin.RefreshToken = newRefreshToken;
            await db.SaveChangesAsync();

            return Results.Json(new { newJwtToken, newRefreshToken });
        });
        
        return group;
    }
    
    static string GenerateJwtToken(User user, string secret)
    {
        var key = Encoding.UTF8.GetBytes(secret);
        var securityKey = new SymmetricSecurityKey(key);
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.Name, user.Username),
            new Claim("Role", user.Role ?? string.Empty),
            new Claim(ClaimTypes.DateOfBirth, $"{user.DateOfBirth}")
        };

        var token = new JwtSecurityToken(
            expires: DateTime.Now.AddMinutes(15),
            signingCredentials: credentials,
            claims: claims);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    static string GenerateRefreshToken()
    {
        var randomNumber = new byte[32];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);
        return Convert.ToBase64String(randomNumber);
    }

    static ClaimsPrincipal GetPrincipalFromExpiredToken(string token, string secret)
    {
        var key = Encoding.UTF8.GetBytes(secret);
        var tokenValidationParameters = new TokenValidationParameters
        {
            ValidateAudience = false,
            ValidateIssuer = false,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(key),
            ValidateLifetime = false
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out SecurityToken securityToken);
        if (securityToken is not JwtSecurityToken jwtSecurityToken 
            || !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
        {
            throw new SecurityTokenException("Invalid token");
        }

        return principal;
    }

}

public record UserRegistrationRequest
{
    public required string Username { get; set; }
    public required string Password { get; set; }
    public DateOnly? DateOfBirth { get; set; }
    public string? Role { get; set; }
}