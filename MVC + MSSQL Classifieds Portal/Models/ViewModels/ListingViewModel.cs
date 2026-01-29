using System.ComponentModel.DataAnnotations;

namespace MVC___MSSQL_Classifieds_Portal.Models.ViewModels
{
    public class ListingViewModel
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public decimal Price { get; set; }
        public string CategoryName { get; set; }
        public string OwnerUsername { get; set; }
        public DateTime CreatedAt { get; set; }
        public Category Category { get; set; }
        public User User { get; set; }
    }

    public class ListingCreateViewModel
    {
        [Required]
        [StringLength(50, MinimumLength = 3)]
        public string Title { get; set; }

        [StringLength(500)]
        public string? Description { get; set; }

        [Required]
        [Range(0.01, 999999.99)]
        public decimal Price { get; set; }

        [Required]
        public int CategoryId { get; set; }

        [Required]
        public int UserId { get; set; }

        [Url(ErrorMessage = "Please enter a valid URL")]
        [StringLength(255)]
        public string? ImageUrl { get; set; }
    }
}