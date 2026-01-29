namespace MVC___MSSQL_Classifieds_Portal.Models.ViewModels
{
    /// <summary>
    /// ViewModel for listing search/filter form
    /// </summary>
    public class ListingFilterViewModel
    {
        public string? SearchTitle { get; set; }
        
        public int? CategoryId { get; set; }
        
        public decimal? MinPrice { get; set; }
        
        public decimal? MaxPrice { get; set; }
        
        public string? SortBy { get; set; } = "newest"; // newest, price_asc, price_desc
        
        public int CurrentPage { get; set; } = 1;
        
        public int PageSize { get; set; } = 10;
        
        // For displaying filter results
        public List<ListingViewModel> Listings { get; set; } = new List<ListingViewModel>();
        
        public int TotalListings { get; set; }
        
        public int TotalPages => (int)Math.Ceiling((double)TotalListings / PageSize);
        
        // For category dropdown
        public List<Category> Categories { get; set; } = new List<Category>();
    }
}
