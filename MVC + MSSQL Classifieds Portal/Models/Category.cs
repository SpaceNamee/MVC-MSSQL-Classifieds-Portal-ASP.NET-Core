using System.ComponentModel.DataAnnotations; // for [Required]


namespace MVC___MSSQL_Classifieds_Portal.Models
{
    public class Category
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; }

        [MaxLength(255)]
        public string? Description { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation
        // new List<Listing>() - prevents null reference errors.
        public ICollection<Listing> Listings { get; set; } = new List<Listing>();  // ICollection<T> in EF Core is simply the type used for navigation properties that represent a one-to-many relationship. One user → many listings
    }
}
