using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MVC___MSSQL_Classifieds_Portal.Models;
using MVC___MSSQL_Classifieds_Portal.Models.ViewModels;
using System.Diagnostics;

namespace MVC___MSSQL_Classifieds_Portal.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ClassifieldsContext _context;

        public HomeController(ILogger<HomeController> logger, ClassifieldsContext context)
        {
            _logger = logger;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var viewModel = new HomeViewModel
            {
                TotalListings = await _context.Listings.CountAsync(),
                TotalCategories = await _context.Categories.CountAsync(),
                TotalUsers = await _context.Users.CountAsync(),

                // Get 6 most recent listings
                RecentListings = await _context.Listings
                    .Include(l => l.Category)
                    .Include(l => l.User)
                    .OrderByDescending(l => l.CreatedAt)
                    .Take(6)
                    .ToListAsync(),

                // Get all categories with their listing counts
                Categories = await _context.Categories
                    .Include(c => c.Listings)
                    .ToListAsync()
            };

            return View(viewModel);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
