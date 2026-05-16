using System.ComponentModel.DataAnnotations;
namespace LibraryApi.DTOs
{
    public class BookDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Author { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }

    public class BookCreateDto
    {
        [Required(ErrorMessage = "Bokens titel är obligatorisk.")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Titeln måste vara mellan 2 och 100 tecken.")]
        public required string Title { get; set; }

        [Required(ErrorMessage = "Författarens namn är obligatoriskt.")]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "Författarens namn måste vara mellan 2 och 50 tecken.")]
        public required string Author { get; set; }
    }

    public class BookUpdateDto
    {
        [Required(ErrorMessage = "Bokens titel är obligatorisk.")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Titeln måste vara mellan 2 och 100 tecken.")]
        public required string Title { get; set; }

        [Required(ErrorMessage = "Författarens namn är obligatoriskt.")]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "Författarens namn måste vara mellan 2 och 50 tecken.")]
        public required string Author { get; set; }
    }

    
}


