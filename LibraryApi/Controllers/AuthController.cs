using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using LibraryApi.Models;
using LibraryApi.Data;
using LibraryApi.Services;
using System.Security.Claims;

namespace LibraryApi.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly TokenService _tokenService;

    public AuthController(ApplicationDbContext context, TokenService tokenService)
    {
        _context = context;
        _tokenService = tokenService;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);

        if (user == null)
        {
            return Unauthorized(new { message = "Invalid email or password!" });
        }

        bool isValidPassword = BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash);

        if (!isValidPassword)
        {
            return Unauthorized(new { message = "Invalid email or password!" });
        }

        var token = _tokenService.CreateToken(user);

        return Ok(new {
            token = token,
            email = user.Email,
            name = user.Name
            });
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        if (await _context.Users.AnyAsync(u => u.Email == request.Email))
        {
            return BadRequest(new { message = "Email already registered!" });
        }

        var hashedPassword = BCrypt.Net.BCrypt.HashPassword(request.Password);

        var user = new User
        {
            Name = request.Name,
            Email = request.Email,
            PasswordHash = hashedPassword,
            CreatedAt = DateTime.UtcNow
        };

        try
        {
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            var token = _tokenService.CreateToken(user);
            return Ok(new {
                token = token,
                email = user.Email,
                name = user.Name,
                message = "User registered successfully!"
            });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Database Error: {ex.Message}");
            return StatusCode(500, new { message = "An error occurred while saving to the database!" });
        }
    }

    [HttpGet("google-login")]
    public IActionResult GoogleLogin()
    {
        var properties = new AuthenticationProperties {
            RedirectUri = "/api/auth/google-response"
        };
        return Challenge(properties, GoogleDefaults.AuthenticationScheme);
    }

    [HttpGet("google-response")]
    public async Task<IActionResult> GoogleResponse()
    {
        var result = await HttpContext.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);

        if (!result.Succeeded || result.Principal == null)
            return BadRequest("Google authentication failed.");

        var email = result.Principal.FindFirst(ClaimTypes.Email)?.Value;
        var name = result.Principal.FindFirst(ClaimTypes.Name)?.Value ?? "Google User";

        if (string.IsNullOrEmpty(email))
            return BadRequest("Could not retrieve email from Google.");

        // Check if user exists in our Supabase database
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);

        if (user == null)
        {
            // Create new user if they don't exist
            user = new User
            {
                Id = Guid.NewGuid(),
                Email = email,
                Name = name,
                PasswordHash = "GOOGLE_AUTH_EXTERNAL",
                CreatedAt = DateTime.UtcNow
            };

            try
            {
                _context.Users.Add(user);
                await _context.SaveChangesAsync();
                Console.WriteLine($"New Google user created: {email}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"DB Error: {ex.Message}");
                return StatusCode(500, "Error saving Google user to database.");
            }
        }
        var token = _tokenService.CreateToken(user);
        var hostValue = Request.Host.Value ?? "";

        var frontendUrl = hostValue.Contains("render.com")
            ? "https://angularredcodecrud-applikation.netlify.app"
            : "http://localhost:4200";

        return Redirect($"{frontendUrl}/login?token={token}&email={email}&success=true");
    }
}