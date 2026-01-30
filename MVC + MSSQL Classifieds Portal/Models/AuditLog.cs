using System.ComponentModel.DataAnnotations; // for [Required]


namespace MVC___MSSQL_Classifieds_Portal.Models
{
    public class AuditLog
    {
        public int Id { get; set; }

        public int? UserId { get; set; }

        [Required]
        [MaxLength(50)]
        public string Action { get; set; }  // CREATE, UPDATE, DELETE, etc.

        [Required]
        [MaxLength(100)]
        public string EntityName { get; set; }  // Listing, User, Category, etc.

        public int? EntityId { get; set; }

        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        public string? Changes { get; set; } // JSON serialized changes

        [Obsolete("Use Changes instead")]
        public string? Details { get; set; } // Legacy field - use Changes instead

        // Navigation
        public User? User { get; set; }
    }

}
