using System.ComponentModel.DataAnnotations; // for [Required]
using System.ComponentModel.DataAnnotations.Schema; // [Column] (and [Key], [ForeignKey], etc.) comes from this


namespace MVC___MSSQL_Classifieds_Portal.Models
{
    public class Listing
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string Title { get; set; }

        [MaxLength(100)]
        public string? Description { get; set; }

        [Required]
        [DataType(DataType.Currency)]
        [Column(TypeName = "decimal(18,4)")]
        public decimal Price { get; set; }

        [Required]
        public int CategoryId {get; set; }
        
        [Required]
        public int UserId { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? LastUpdatedAt { get; set; }

        public bool IsActive { get; set; } = true;

        [MaxLength(255)]
        public string? ImageUrl { get; set; } // optional image

        // Navigation
        public Category Category { get; set; } //
        public User User { get; set; }
    }
}
