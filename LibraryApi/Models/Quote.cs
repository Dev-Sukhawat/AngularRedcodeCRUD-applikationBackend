using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace LibraryApi.Models;

[Table("quotes")]
public class Quote
{
    [Key]
    [Column("id")]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Column("title")]
    public string? Title { get; set; } = string.Empty;

    [Required]
    [Column("text")]
    public string Text { get; set; } = string.Empty;

    [Column("page_number")]
    public int? PageNumber { get; set; }

    [Column("book_id")]
    public Guid? BookId { get; set; }

    [Required]
    [Column("user_id")]
    public Guid UserId { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Prevent circular reference during JSON serialization
    [JsonIgnore]
    public Book? Book { get; set; }
}