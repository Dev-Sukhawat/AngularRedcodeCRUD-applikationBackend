using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication.Cookies;
using LibraryApi.Models;



namespace LibraryApi.Controllers;
[ApiController]
[Route("api/[controller]")]

public class AuthController : ControllerBase
{
    [HttpPost("login")]
    public IActionResult Login([FromBody] LoginRequest request)
    {
        if (request.Email == "test@test.com" && request.Password == "123")
        {
            return Ok(new { token = "fake-jwt-token", email = request.Email });
        }
        return Unauthorized(new { message = "Fel uppgifter!" });
    }

    [HttpPost("register")]
    public IActionResult Register([FromBody] RegisterRequest request)
    {
        if (request.Name != null && request.Email == "test@test.com" && request.Password == "123")
        {
            return Ok(new { token = "fake-jwt-token", email = request.Email });
        }
        return Unauthorized(new { message = "Fel uppgifter!" });
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

        // Skicka användaren tillbaka till Angular (localhost:4200)
        // Vi skickar med e-posten i URL:en bara för att se att det fungerar just nu
        return Redirect($"http://localhost:4200/login?email={email}&success=true");
    }
}