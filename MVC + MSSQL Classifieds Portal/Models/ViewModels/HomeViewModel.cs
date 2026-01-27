using MVC___MSSQL_Classifieds_Portal.Models;

namespace MVC___MSSQL_Classifieds_Portal.Models.ViewModels
{
    public class HomeViewModel
    {
        public int TotalListings { get; set; }
        public int TotalCategories { get; set; }
        public int TotalUsers { get; set; }
        public List<Listing> RecentListings { get; set; } = new List<Listing>();
        public List<Category> Categories { get; set; } = new List<Category>();
    }
}