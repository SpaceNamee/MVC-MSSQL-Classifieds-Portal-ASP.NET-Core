using System.ComponentModel.DataAnnotations; // for [Required]
using System.ComponentModel.DataAnnotations.Schema; // [Column] (and [Key], [ForeignKey], etc.) comes from this


namespace MVC___MSSQL_Classifieds_Portal.Models
{   
    public class Listing
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Title is required")]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "Title must be 3-50 characters")]
        [Display(Name = "Listing Title")]
        public string Title { get; set; }

        [MaxLength(100)]
        public string? Description { get; set; }

        [Required(ErrorMessage = "Price is required")]
        [Range(0.01, 999999.99, ErrorMessage = "Price must be $0.01 - $999,999.99")]
        [DataType(DataType.Currency)]
        [Column(TypeName = "decimal(18,4)")]
        public decimal Price { get; set; }

        [Required(ErrorMessage = "Please select a category")]
        [Display(Name = "Category")]
        public int CategoryId {get; set; }

        [Required(ErrorMessage = "Please select an owner")]
        [Display(Name = "Owner")]

        public int UserId { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? LastUpdatedAt { get; set; }

        public bool IsActive { get; set; } = true;

        [Url(ErrorMessage = "Please enter a valid URL")]
        [StringLength(255)]
        public string? ImageUrl { get; set; } // optional image

        // Navigation
        public Category Category { get; set; } //
        public User User { get; set; }
    }
}
