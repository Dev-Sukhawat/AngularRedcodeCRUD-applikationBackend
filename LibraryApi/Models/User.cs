using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LibraryApi.Models;

[Table("users")] // Berättar för .NET att tabellen heter "users" i Supabase
public class User
{
    [Key] // Markerar att detta är ID-kolumnen
    [Column("id")]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Column("name")]
    public string Name { get; set; } = string.Empty;

    [Column("email")]
    public string Email { get; set; } = string.Empty;

    [Column("password_hash")]
    // Om din kolumn i Supabase heter "password_hash", använd [Column] nedan
    public string PasswordHash { get; set; } = string.Empty;

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}