using System.Security.Claims;
using LibraryApi.Data;
using LibraryApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LibraryApi.DTOs;

namespace LibraryApi.Controllers;

[Authorize(AuthenticationSchemes = Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults.AuthenticationScheme)]
[ApiController]
[Route("api/[controller]")]
public class QuotesController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public QuotesController(ApplicationDbContext context)
    {
        _context = context;
    }

    // 1. Get all quotes
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Quote>>> QuoteDto()
    {
        var userId = GetUserId();

        var quotes = await _context.Quotes
            .Where(q => q.UserId == userId)
            .OrderByDescending(q => q.CreatedAt)
            .ToListAsync();

        return Ok(quotes);
    }

    // 2. Create a new quote
    [HttpPost]
    public async Task<ActionResult<Quote>> CreateQuote([FromBody] QuoteCreateDto dto)
    {
        var userId = GetUserId();

        if (string.IsNullOrWhiteSpace(dto.Text))
        {
            return BadRequest("Citattexten får inte vara tom.");
        }

        var quote = new Quote
        {
            Text = dto.Text,
            Title = dto.Title?.Trim() != "" ? dto.Title : null,
            PageNumber = dto.PageNumber ?? null,
            UserId = userId,
            BookId = null,
            CreatedAt = DateTime.UtcNow
        };

        _context.Quotes.Add(quote);
        await _context.SaveChangesAsync();

        return Ok(quote);
    }

    // 2. Update an existing quote
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateQuote(Guid id, [FromBody] QuoteUpdateDto quoteUpdate)
    {
        var userId = GetUserId();

        // Find the quote and check if it belongs to the user
        var quote = await _context.Quotes
            .FirstOrDefaultAsync(q => q.Id == id && q.UserId == userId);

        if (quote == null)
        {
            return NotFound("Quote not found or you don't have permission to update it.");
        }

        quote.Text = quoteUpdate.Text;
        quote.Title = quoteUpdate.Title;
        quote.PageNumber = quoteUpdate.PageNumber;

        _context.Quotes.Update(quote);
        await _context.SaveChangesAsync();

        return Ok(quote);
    }

    // 3. Delete a quote
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteQuote(Guid id)
    {
        var userId = GetUserId();

        var quote = await _context.Quotes
            .FirstOrDefaultAsync(q => q.Id == id && q.UserId == userId);

        if (quote == null)
        {
            return NotFound("Quote not found or you don't have permission to delete it.");
        }

        _context.Quotes.Remove(quote);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    // Helper method to extract User ID from JWT
    private Guid GetUserId()
    {
        var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        
        if (string.IsNullOrEmpty(userIdString) || !Guid.TryParse(userIdString, out Guid userId))
        {
            throw new UnauthorizedAccessException("Invalid user ID in token.");
        }
        
        return userId;
    }
}