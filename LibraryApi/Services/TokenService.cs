using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using LibraryApi.Models;
using Microsoft.IdentityModel.Tokens;

namespace LibraryApi.Services;

public class TokenService
{
    private readonly IConfiguration _config;

    public TokenService(IConfiguration config)
    {
        _config = config;
    }

    public string CreateToken(User user)
    {
        // 1. Define claims (the information stored inside the token)
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Name, user.Name)
        };

        var jwtKey = Environment.GetEnvironmentVariable("JWT_KEY");

        if (string.IsNullOrEmpty(jwtKey))
        {
            throw new Exception("JWT_KEY could not be found in Environment Variables!");
        }

        // 2. Get the secret key from .env/configuration
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey ?? ""));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        // 3. Create the token details
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.Now.AddHours(1),
            SigningCredentials = creds,
            Issuer = _config["JWT_ISSUER"],
            Audience = _config["JWT_AUDIENCE"]
        };

        // 4. Generate and return the final string
        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);

        return tokenHandler.WriteToken(token);
    }
}