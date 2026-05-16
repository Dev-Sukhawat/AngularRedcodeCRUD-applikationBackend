using System.Security.Claims;
using LibraryApi.Data;
using LibraryApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LibraryApi.DTOs;

namespace LibraryApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
public class BooksController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public BooksController(ApplicationDbContext context)
    {
        _context = context;
    }

    // 1. HÄMTA ALLA BÖCKER FÖR DEN INLOGGADE ANVÄNDAREN
    [HttpGet]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public async Task<ActionResult<IEnumerable<Book>>> GetBooks()
    {
        var userId = GetUserId();
                // var userId = Guid.Parse("d8a44c1d-51f6-4476-9b7a-e1cc9abe2e92"); // Hårdkoda ett giltigt UUID under testet
        
        // Hämta bara böcker som tillhör denna specifika användare
        // Vi inkluderar även tillhörande citat (.Include)
        var books = await _context.Books
            .Include(b => b.Quotes)
            .Where(b => b.UserId == userId)
            .OrderByDescending(b => b.CreatedAt)
            .ToListAsync();

        return Ok(books);
    }

    // 2. SKAPA EN NY BOK
    [HttpPost]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public async Task<ActionResult<Book>> CreateBook([FromBody] BookCreateDto bookDto)
    {
        try
        {
            if (bookDto == null)
            {
                return BadRequest(new { error = "No data sent from Angular." });
            }

            Guid userId;
            try
            {
                userId = GetUserId();

            }
            catch (Exception authEx)
            {
                return StatusCode(500, new { error = "Failed to extract user ID from token.", details = authEx.Message });
            }

            var book = new Book
            {
                Title = bookDto.Title,
                Author = bookDto.Author,
                UserId = userId,
                CreatedAt = DateTime.UtcNow
            };

            try
            {
                _context.Books.Add(book);
                await _context.SaveChangesAsync();
            }
            catch (Exception dbEx)
            {
                var innerMessage = dbEx.InnerException != null ? dbEx.InnerException.Message : "No inner exception";
                return StatusCode(500, new { error = "Failed to save book to database.", details = dbEx.Message, inner = innerMessage });
            }

            return Ok(new { 
                id = book.Id, 
                title = book.Title, 
                author = book.Author, 
                userId = book.UserId 
            });
    } catch (Exception globalEx){
        return StatusCode(500, new { error = "Ett oväntat globalt fel uppstod", details = globalEx.Message });
    }
}

    // UpdateBook_Method:
    [HttpPut("{id}")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public async Task<IActionResult> UpdateBook(Guid id, [FromBody] BookUpdateDto bookDto)
    {
        try
        {
            var userId = GetUserId();

            var book = await _context.Books.FindAsync(id);

            if (book == null)
            {
                return NotFound(new{error = "Book not found or you don't have permission to edit it."});
            }

            if (book.UserId != userId)
            {
                return Forbid();
            }

            book.Title = bookDto.Title;
            book.Author = bookDto.Author;

            _context.Books.Update(book);
            await _context.SaveChangesAsync();

            return NoContent();
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Failed to update book.", detail = ex.Message });
        }
    }

    // DeleteBook_Method:
    [HttpDelete("{id}")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public async Task<IActionResult> DeleteBook(Guid id)
    {
        try
        {
            var userId = GetUserId();

            var book = await _context.Books.FindAsync(id);

            if (book == null)
            {
                return NotFound("Book not found or you don't have permission to delete it.");
            }

            if (book.UserId != userId)
            {
                return Forbid();
            }

            _context.Books.Remove(book);
            await _context.SaveChangesAsync();

            return NoContent();
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Failed to delete book.", detail = ex.Message });
        }
    }

    // En liten hjälpmetod för att plocka ut NameIdentifier (User ID) ur JWT-token
    private Guid GetUserId()
    {
        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

    if (string.IsNullOrEmpty(userIdClaim))
    {
        userIdClaim = User.FindFirst("nameid")?.Value;
    }

    if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
    {
        throw new UnauthorizedAccessException("Invalid user ID in token.");
    }
        
        return userId;
    }
}