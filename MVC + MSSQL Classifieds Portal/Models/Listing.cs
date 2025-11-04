using System.ComponentModel.DataAnnotations; // for [Required]

namespace MVC___MSSQL_Classifieds_Portal.Models
{
    public class Listing
    {
        public int Id { get; set; }

        public string Title { get; set; }

        public string Description { get; set; }

        public float Price { get; set; }

        public int CategoryId {get; set; }

        public int UserId { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime LastUpdatedAt { get; set; }

        // image
    }
}
