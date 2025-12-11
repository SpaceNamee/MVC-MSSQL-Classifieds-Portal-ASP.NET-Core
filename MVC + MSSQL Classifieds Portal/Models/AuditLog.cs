using System.ComponentModel.DataAnnotations; // for [Required]


namespace MVC___MSSQL_Classifieds_Portal.Models
{
    public class AuditLog
    {
        public int Id { get; set; }

        public int UserId { get; set; }

        [Required]
        [MaxLength(50)]
        public string Action { get; set; }  // Created, Updated, Deleted


        [Required]
        public int EntityId { get; set; }

        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        public string? Details { get; set; } // JSON, notes, etc.

        // Navigation
        public User User { get; set; }
    }

}
