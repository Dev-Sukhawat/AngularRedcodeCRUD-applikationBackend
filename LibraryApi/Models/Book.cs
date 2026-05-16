using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LibraryApi.Models;

[Table("books")] // Matchar tabellnamnet i Supabase
public class Book
{
    [Key]
    [Column("id")]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    [Column("title")]
    public string Title { get; set; } = string.Empty;

    [Required]
    [Column("author")]
    public string Author { get; set; } = string.Empty;

    [Required]
    [Column("user_id")]
    public Guid UserId { get; set; } // Kopplingen till den inloggade användaren

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // En bok kan ha många citat (Relation)
    public ICollection<Quote> Quotes { get; set; } = new List<Quote>();
}