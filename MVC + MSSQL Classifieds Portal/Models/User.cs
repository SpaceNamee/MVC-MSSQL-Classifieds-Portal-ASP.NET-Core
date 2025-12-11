using System.ComponentModel.DataAnnotations; // for [Required]

namespace MVC___MSSQL_Classifieds_Portal.Models
{
    public class User
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string Username { get; set; }
        
        [Required]
        [MaxLength(100)]
        public string Email { get; set; }

        [Required]
        public string PasswordHash { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? LastLoginAt { get; set; }

        public bool IsActive { get; set; } = true;

        // Navigation
        public ICollection<Listing> Listings { get; set; } = new List<Listing>(); // ICollection<T> in EF Core is simply the type used for navigation properties that represent a one-to-many relationship. One user → many listings
        public ICollection<AuditLog> AuditLogs { get; set; } = new List<AuditLog>();
    }
}
