using System.ComponentModel.DataAnnotations; // for [Required]


namespace MVC___MSSQL_Classifieds_Portal.Models
{
    public class Category
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

    }
}
