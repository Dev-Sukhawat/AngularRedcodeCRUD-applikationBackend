using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using LibraryApi.Models;
using LibraryApi.Data;


namespace LibraryApi.Controllers;
[ApiController]
[Route("api/auth")]

public class AuthController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    public AuthController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);

        if (user == null)
        {
            return Unauthorized(new { message = "Fel e-post eller lösenord!" });
        }

        bool isValidPassword = BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash);

        if (!isValidPassword)
        {
            return Unauthorized(new { message = "Fel e-post eller lösenord!" });
        }

        return Ok(new { token = "fake-jwt-token", email = user.Email, name = user.Name });
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        if (await _context.Users.AnyAsync(u => u.Email == request.Email))
        {
            return BadRequest(new { message = "E-post redan registrerad!" });
        }

        var hashedPassword = BCrypt.Net.BCrypt.HashPassword(request.Password);

        var user = new User
        {
            Name = request.Name,
            Email = request.Email,
            PasswordHash = hashedPassword
        };

        try
        {
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            return Ok(new { message = "Användare har registrerad!", email = user.Email, name = user.Name });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"DB Error: {ex.Message}");
            return StatusCode(500, new { message = "Något gick fel vid registrering!" });
        }
    }

    [HttpGet("google-login")]
    public IActionResult GoogleLogin()
    {
        // Denna metod skickar användaren till Googles inloggningssida
        var properties = new AuthenticationProperties {
            RedirectUri = Url.Action("GoogleResponse")
        };
        return Challenge(properties, GoogleDefaults.AuthenticationScheme);
    }

    [HttpGet("google-response")]
    public async Task<IActionResult> GoogleResponse()
    {
        // Här landar man efter att ha loggat in hos Google
        var result = await HttpContext.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);

        if (!result.Succeeded)
            return BadRequest("Google-autentisering misslyckades.");

        // Hämta e-postadressen från Google-kontot
        var email = result.Principal.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;
        var name = result.Principal.FindFirst(System.Security.Claims.ClaimTypes.Name)?.Value;
        if (email == null)
            return BadRequest("Google-kontot saknar e-postadress.");
        // Skicka användaren tillbaka till Angular (localhost:4200)
        // Vi skickar med e-posten i URL:en bara för att se att det fungerar just nu
        return Redirect($"http://localhost:4200/login?email={email}&name={name}&success=true");
    }
}