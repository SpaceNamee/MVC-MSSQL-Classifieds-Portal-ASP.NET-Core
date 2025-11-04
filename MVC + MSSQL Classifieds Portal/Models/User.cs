using System.ComponentModel.DataAnnotations; // for [Required]

namespace MVC___MSSQL_Classifieds_Portal.Models
{
    public class User
    {
        public int Id { get; set; }
        
        [Required]
        public string UserName { get; set; }
        public string Email { get; set; }

        [Required]
        public string PasswordHash { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime LastLoginAt { get; set; }

        public bool IsActive { get; set; }

    }
}
