using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MVC___MSSQL_Classifieds_Portal.Models;
using System.Security.Claims;

namespace MVC___MSSQL_Classifieds_Portal.Controllers
{
    public class UsersController : Controller
    {
        private readonly ClassifieldsContext _context;

        public UsersController(ClassifieldsContext context)
        {
            _context = context;
        }

        // =================== USER PROFILE: Personal user page ===================
        [Authorize]
        public async Task<IActionResult> Index()
        {
            // Redirect to personal profile instead of showing all users
            return RedirectToAction(nameof(Profile));
        }

        [Authorize]
        public async Task<IActionResult> Profile()
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            if (userId == 0)
            {
                return Challenge();
            }

            var user = await _context.Users
                .AsNoTracking()
                .Include(u => u.Listings).ThenInclude(l => l.Category)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null) return NotFound();
            return View(user);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var user = await _context.Users
                .AsNoTracking()
                .Include(u => u.Listings).ThenInclude(l => l.Category)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (user == null) return NotFound();
            return View(user);
        }

        public IActionResult Create() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Username,Email,PasswordHash")] User user)
        {
            // Check duplicates
            if (await _context.Users.AnyAsync(u => u.Username == user.Username))
                ModelState.AddModelError("Username", "Username already exists.");
            if (await _context.Users.AnyAsync(u => u.Email == user.Email))
                ModelState.AddModelError("Email", "Email already registered.");

            if (ModelState.IsValid)
            {
                user.CreatedAt = DateTime.UtcNow;
                user.IsActive = true;
                user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(user.PasswordHash);
                _context.Add(user);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Profile));
            }
            return View(user);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var user = await _context.Users.FindAsync(id);
            if (user == null) return NotFound();
            return View(user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Username,Email,PasswordHash,CreatedAt,IsActive")] User user)
        {
            if (id != user.Id) return NotFound();

            // Check duplicates (excluding self)
            if (await _context.Users.AnyAsync(u => u.Username == user.Username && u.Id != user.Id))
                ModelState.AddModelError("Username", "Username already exists.");

            if (ModelState.IsValid)
            {
                _context.Update(user);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Profile));
            }
            return View(user);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var user = await _context.Users.Include(u => u.Listings).FirstOrDefaultAsync(m => m.Id == id);
            if (user == null) return NotFound();
            return View(user);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var user = await _context.Users.Include(u => u.Listings).FirstOrDefaultAsync(u => u.Id == id);
            if (user != null)
            {
                // Soft delete user and their listings
                user.IsActive = false;
                foreach (var listing in user.Listings)
                    listing.IsActive = false;
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Profile));
        }
    }
}