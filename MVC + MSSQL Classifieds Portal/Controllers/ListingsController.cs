using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MVC___MSSQL_Classifieds_Portal.Models;
using MVC___MSSQL_Classifieds_Portal.Models.ViewModels;
using System.Security.Claims;


namespace MVC___MSSQL_Classifieds_Portal.Controllers
{
    public class ListingsController : Controller
    {
        private readonly ClassifieldsContext _context;
        private readonly IMapper _mapper;

        // Constructor - Dependency Injection gives us the database context
        public ListingsController(ClassifieldsContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        // =================== READ: List all listings with filtering & pagination ===================
        // GET: Listings
        public async Task<IActionResult> Index(
            string? searchTitle,
            int? categoryId,
            decimal? minPrice,
            decimal? maxPrice,
            string? sortBy,
            int page = 1)
        {
            const int pageSize = 10;
            
            // Start with base query - Include() loads related data (Category, User) - "Eager Loading"
            IQueryable<Listing> query = _context.Listings
                .Where(l => l.IsActive)
                .Include(l => l.Category)
                .Include(l => l.User);

            // Apply filters
            if (!string.IsNullOrEmpty(searchTitle))
            {
                query = query.Where(l => l.Title.Contains(searchTitle));
            }

            if (categoryId.HasValue && categoryId > 0)
            {
                query = query.Where(l => l.CategoryId == categoryId);
            }

            if (minPrice.HasValue)
            {
                query = query.Where(l => l.Price >= minPrice);
            }

            if (maxPrice.HasValue)
            {
                query = query.Where(l => l.Price <= maxPrice);
            }

            // Apply sorting
            query = sortBy switch
            {
                "price_asc" => query.OrderBy(l => l.Price),
                "price_desc" => query.OrderByDescending(l => l.Price),
                _ => query.OrderByDescending(l => l.CreatedAt) // default: newest first
            };

            // Get total count before pagination
            var totalListings = await query.CountAsync();

            // Apply pagination
            var listings = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var viewModels = _mapper.Map<List<ListingViewModel>>(listings);
            var categories = await _context.Categories.ToListAsync();

            var filterViewModel = new ListingFilterViewModel
            {
                Listings = viewModels,
                TotalListings = totalListings,
                CurrentPage = page,
                PageSize = pageSize,
                SearchTitle = searchTitle,
                CategoryId = categoryId,
                MinPrice = minPrice,
                MaxPrice = maxPrice,
                SortBy = sortBy ?? "newest",
                Categories = categories
            };

            return View(filterViewModel);
        }

        // =================== READ: Single listing details ===================
        // GET: Listings/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var listring = await _context.Listings
                .Include(l => l.Category)
                .Include(l => l.User)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (listring == null)
            {
                return NotFound();
            }

            return View(listring);
        }

        // =================== CREATE: Show empty form ===================
        // GET: Listings/Create
        [Authorize]
        public IActionResult Create()
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            if (userId == 0)
            {
                return Challenge();
            }
            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name");

            return View(new Listing { UserId = userId });
        }

        // =================== CERATE: Save new listing ===================
        // POST: Listings/Create
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Title,Description,Price,CategoryId,ImageUrl")] Listing listing)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            if (userId == 0)
            {
                return Challenge();
            }
            listing.UserId = userId;
            ModelState.Remove("UserId");

            if (ModelState.IsValid)
            {
                listing.CreatedAt = DateTime.UtcNow;
                listing.IsActive = true;

                _context.Add(listing);
                await _context.SaveChangesAsync();

                return RedirectToAction("Index");
            }

            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name", listing.CategoryId);

            return View(listing);
        }

        // =================== UPDATE: Show form with existing data ===================
        // GET: Listings/Edit/5
        [Authorize]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) { return NotFound(); }

            var listing = await _context.Listings.FindAsync(id);

            if (listing == null) { return NotFound(); }

            // Check if current user is the owner
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            if (listing.UserId != userId)
            {
                return Forbid();
            }

            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name", listing.CategoryId);
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Username", listing.UserId);

            return View(listing);
        }

        // =================== Update: Save changes ===================
        // POST: Listings/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Title,Description,Price,CategoryId,UserId,CreatedAt,IsActive,ImageUrl,LastUpdatedAt")] Listing listing)
        {
            if (id != listing.Id)
            {
                return NotFound();
            }

            var existingListing = await _context.Listings.FindAsync(id);
            if (existingListing == null)
            {
                return NotFound();
            }

            // Check if current user is the owner
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            if (existingListing.UserId != userId)
            {
                return Forbid();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    existingListing.Title = listing.Title;
                    existingListing.Description = listing.Description;
                    existingListing.Price = listing.Price;
                    existingListing.CategoryId = listing.CategoryId;
                    existingListing.ImageUrl = listing.ImageUrl;
                    existingListing.LastUpdatedAt = DateTime.UtcNow;

                    _context.Update(existingListing);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ListingExists(existingListing.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }

            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name", listing.CategoryId);

            return View(existingListing);
        }

        // ========== DELETE: Show confirmation page ==========
        // GET: Listings/Delete/5
        [Authorize]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var listing = await _context.Listings
                .Include(l => l.Category)
                .Include(l => l.User)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (listing == null)
            {
                return NotFound();
            }

            // Check if current user is the owner
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            if (listing.UserId != userId)
            {
                return Forbid();
            }

            return View(listing);
        }

        // ========== DELETE: Actually remove the record ==========
        // POST: Listings/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var listing = await _context.Listings.FindAsync(id);

            if (listing == null)
            {
                return NotFound();
            }

            // Check if current user is the owner
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            if (listing.UserId != userId)
            {
                return Forbid();
            }

            // SOFT DELETE: mark as inactive instead of removing
            listing.IsActive = false;
            listing.LastUpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ListingExists(int id)
        {
            return _context.Listings.Any(e => e.Id == id);
        }
    }
}
