using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MVC___MSSQL_Classifieds_Portal.Models;
using System.Security.Claims;

namespace MVC___MSSQL_Classifieds_Portal.Controllers
{
    public class AccountController : Controller
    {
        private readonly ClassifieldsContext _context;

        public AccountController(ClassifieldsContext context)
        {
            _context = context;
        }

        // GET: Account/Register
        public IActionResult Register()
        {
            return View();
        }

        // POST: Account/Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(string username, string email, string password, string confirmPassword)
        {
            // Validation
            if (string.IsNullOrEmpty(username) || username.Length < 3)
            {
                ModelState.AddModelError("username", "Username must be at least 3 characters");
            }

            if (string.IsNullOrEmpty(email) || !email.Contains("@"))
            {
                ModelState.AddModelError("email", "Please enter a valid email");
            }

            if (password != confirmPassword)
            {
                ModelState.AddModelError("password", "Passwords don't match");
            }

            if (string.IsNullOrEmpty(password) || password.Length < 6)
            {
                ModelState.AddModelError("password", "Password must be at least 6 characters");
            }

            // Check if username already exists
            if (await _context.Users.AnyAsync(u => u.Username == username))
            {
                ModelState.AddModelError("username", "Username already exists");
            }

            // Check if email already exists
            if (await _context.Users.AnyAsync(u => u.Email == email))
            {
                ModelState.AddModelError("email", "Email already registered");
            }

            if (!ModelState.IsValid)
            {
                return View();
            }

            // Hash password
            string passwordHash = BCrypt.Net.BCrypt.HashPassword(password);

            // Create new user
            var user = new User
            {
                Username = username,
                Email = email,
                PasswordHash = passwordHash,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // Auto-login after registration
            await SignInUser(user);
            return RedirectToAction("Index", "Home");
        }

        // GET: Account/Login
        public IActionResult Login()
        {
            return View();
        }

        // POST: Account/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(string username, string password)
        {
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                ModelState.AddModelError("", "Username and password are required");
                return View();
            }

            // Find user
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);

            if (user == null || !BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
            {
                ModelState.AddModelError("", "Invalid username or password");
                return View();
            }

            if (!user.IsActive)
            {
                ModelState.AddModelError("", "User account is inactive");
                return View();
            }

            // Update last login
            user.LastLoginAt = DateTime.UtcNow;
            _context.Update(user);
            await _context.SaveChangesAsync();

            // Sign in user
            await SignInUser(user);
            return RedirectToAction("Index", "Home");
        }

        // GET: Account/Logout
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "Home");
        }

        // GET: Account/AccessDenied
        [HttpGet("/Account/AccessDenied")]
        public IActionResult AccessDenied()
        {
            return View();
        }

        // Helper method to sign in user with cookie
        private async Task SignInUser(User user)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Email, user.Email)
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var authProperties = new AuthenticationProperties
            {
                IsPersistent = true,
                ExpiresUtc = DateTimeOffset.UtcNow.AddDays(7)
            };

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                authProperties);
        }
    }
}
