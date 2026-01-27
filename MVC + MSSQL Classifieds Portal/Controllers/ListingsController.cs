using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MVC___MSSQL_Classifieds_Portal.Models;
using MVC___MSSQL_Classifieds_Portal.Models.ViewModels;


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

        // =================== READ: List all listings ===================
        // GET: Listings
        public async Task<IActionResult> Index()
        {
            // Include() is LINQ - it loads realted data (Category, User)
            // This is called "Eager Loading"
            var listings = await _context.Listings.Include(l => l.Category).Include(l => l.User).ToListAsync();
            var viewModels = _mapper.Map<List<ListingViewModel>>(listings);
            return View(viewModels);
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
        // GET: Listings/Details/5
        public IActionResult Create()
        {
            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name");
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Username");

            return View();
        }

        // =================== CERATE: Save new listing ===================
        // POST: Listings/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Title,Description,Price,CategoryId,UserId,ImageUrl")] Listing listing)
        {
            if (ModelState.IsValid)
            {
                listing.CreatedAt = DateTime.UtcNow;
                listing.IsActive = true;

                _context.Add(listing);
                await _context.SaveChangesAsync();

                return RedirectToAction("Index");
            }

            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name", listing.CategoryId);
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Username", listing.UserId);

            return View(listing);
        }

        // =================== UPDATE: Show form with existing data ===================
        // GET: Listings/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) { return NotFound(); }

            var listing = await _context.Listings.FindAsync(id);

            if (listing == null) { return NotFound(); }

            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name", listing.CategoryId);
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Username", listing.UserId);

            return View(listing);
        }

        // =================== Update: Save changes ===================
        // POST: Listings/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Title,Descriptions,Price,CategoryId,UserId,CreatedAt,IsActive")] Listing listing)
        {
            if (id != listing.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    listing.LastUpdatedAt = DateTime.UtcNow;

                    _context.Update(listing);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ListingExists(listing.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }

                return RedirectToAction(nameof(Index));
            }

            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name", listing.CategoryId);
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Username", listing.UserId);

            return View(listing);
        }

        // ========== DELETE: Show confirmation page ==========
        // GET: Listings/Delete/5
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

            return View(listing);
        }

        // ========== DELETE: Actually remove the record ==========
        // POST: Listings/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var listing = await _context.Listings.FindAsync(id);

            if (listing != null)
            {
                // SOFT DELETE: mark as inactive instead of removing
                listing.IsActive = false;
                listing.LastUpdatedAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ListingExists(int id)
        {
            return _context.Listings.Any(e => e.Id == id);
        }
    }
}
