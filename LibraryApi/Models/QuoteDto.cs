using System.ComponentModel.DataAnnotations;

namespace LibraryApi.DTOs
{
    public class QuoteDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Text { get; set; } = string.Empty;
        public Guid? BookId { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class QuoteCreateDto
    {
        [Required(ErrorMessage = "Quote text is required.")]
        [StringLength(500, MinimumLength = 2, ErrorMessage = "Quote must be between 2 and 500 characters.")]
        public required string Text { get; set; }

        [StringLength(100, MinimumLength = 2, ErrorMessage = "Title must be between 2 and 100 characters.")]
        public string? Title { get; set; } = string.Empty;

        public int? PageNumber { get; set; }

    }

    public class QuoteUpdateDto
    {
        [Required(ErrorMessage = "Quote text is required.")]
        [StringLength(500, MinimumLength = 2, ErrorMessage = "Quote must be between 2 and 500 characters.")]
        public required string Text { get; set; }

        [StringLength(100, MinimumLength = 2, ErrorMessage = "Title must be between 2 and 100 characters.")]
        public string? Title { get; set; } = string.Empty;

        public int? PageNumber { get; set; }
    }
}