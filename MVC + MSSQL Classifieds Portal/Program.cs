using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using MVC___MSSQL_Classifieds_Portal.Models;
using MVC___MSSQL_Classifieds_Portal.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddAutoMapper(typeof(Program));

// Register custom services
builder.Services.AddScoped<IAuditLogService, AuditLogService>();

// Configure Cookie Authentication
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.LogoutPath = "/Account/Logout";
        options.AccessDeniedPath = "/Account/AccessDenied";
        options.ExpireTimeSpan = TimeSpan.FromDays(7);
        options.SlidingExpiration = true;
    });

builder.Services.AddControllersWithViews(options =>
{
    options.Filters.Add(new AutoValidateAntiforgeryTokenAttribute());
});

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = null;
    });


builder.Services.AddDbContext<ClassifieldsContext>(options =>
    options.UseSqlServer("server=(localdb)\\mssqllocaldb;Database=ClassifieldsContext;Trusted_Connection=True;TrustServerCertificate=True")
);


var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ClassifieldsContext>();

    // Ensure database is created and up to date
    try
    {
        context.Database.Migrate();
    }
    catch
    {
        context.Database.EnsureCreated();
    }

    // ---- Add Users ----
    if (!context.Users.Any()) // prevent duplicate inserts.
    {
        var aliceHash = BCrypt.Net.BCrypt.HashPassword("Alice123!");
        var bobHash = BCrypt.Net.BCrypt.HashPassword("Bob123!");
        var user1 = new User
        {
            Username = "alice",
            Email = "alice@example.com",
            PasswordHash = aliceHash,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
        var user2 = new User
        {
            Username = "bob",
            Email = "bob@example.com",
            PasswordHash = bobHash,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
        context.Users.AddRange(user1, user2);
        context.SaveChanges();
    }
    else
    {
        // Fix legacy seed users with non-BCrypt hashes
        var alice = context.Users.AsNoTracking().FirstOrDefault(u => u.Username == "alice");
        if (alice != null && (alice.PasswordHash == null || !alice.PasswordHash.StartsWith("$2")))
        {
            alice.PasswordHash = BCrypt.Net.BCrypt.HashPassword("Alice123!");
            alice.IsActive = true;
        }

        var bob = context.Users.AsNoTracking().FirstOrDefault(u => u.Username == "bob");
        if (bob != null && (bob.PasswordHash == null || !bob.PasswordHash.StartsWith("$2")))
        {
            bob.PasswordHash = BCrypt.Net.BCrypt.HashPassword("Bob123!");
            bob.IsActive = true;
        }

        context.SaveChanges();
    }

    // ---- Add Categories ----
    if (!context.Categories.Any())
    {
        var cat1 = new Category { Name = "Electronics", Description = "Phones, laptops" };
        var cat2 = new Category { Name = "Furniture", Description = "Chairs, tables" };
        context.Categories.AddRange(cat1, cat2);
        context.SaveChanges();
    }

    // ---- Add Listings ----
    if (!context.Listings.Any())
    {
        var alice = context.Users.AsNoTracking().First(u => u.Username == "alice");
        var bob = context.Users.AsNoTracking().First(u => u.Username == "bob");
        var electronics = context.Categories.AsNoTracking().First(c => c.Name == "Electronics");
        var furniture = context.Categories.AsNoTracking().First(c => c.Name == "Furniture");

        var listings = new List<Listing>
        {
            // Alice's listings
            new Listing
            {
                Title = "iPhone 14",
                Description = "Like new, 256GB, mint condition",
                Price = 799.99m,
                UserId = alice.Id,
                CategoryId = electronics.Id,
                ImageUrl = "https://example.com/iphone14.jpg"
            },
            new Listing
            {
                Title = "MacBook Pro 14\"",
                Description = "M2 Max, 16GB RAM, 512GB SSD, 2022 model",
                Price = 1899.50m,
                UserId = alice.Id,
                CategoryId = electronics.Id,
                ImageUrl = "https://example.com/macbook.jpg"
            },
            new Listing
            {
                Title = "AirPods Pro",
                Description = "Active noise cancellation, original box",
                Price = 199.99m,
                UserId = alice.Id,
                CategoryId = electronics.Id,
                ImageUrl = "https://example.com/airpods.jpg"
            },
            new Listing
            {
                Title = "Leather Sofa",
                Description = "Brown leather, 3-seater, very comfortable",
                Price = 1200.00m,
                UserId = alice.Id,
                CategoryId = furniture.Id,
                ImageUrl = "https://example.com/sofa.jpg"
            },
            new Listing
            {
                Title = "Dining Table Set",
                Description = "Wooden table with 6 chairs, perfect condition",
                Price = 450.00m,
                UserId = alice.Id,
                CategoryId = furniture.Id,
                ImageUrl = "https://example.com/dining.jpg"
            },
            
            // Bob's listings
            new Listing
            {
                Title = "Samsung Galaxy S23",
                Description = "Phantom Black, 256GB, like new",
                Price = 699.99m,
                UserId = bob.Id,
                CategoryId = electronics.Id,
                ImageUrl = "https://example.com/galaxy.jpg"
            },
            new Listing
            {
                Title = "iPad Air 5th Gen",
                Description = "64GB, Space Gray, with Apple Pencil",
                Price = 549.99m,
                UserId = bob.Id,
                CategoryId = electronics.Id,
                ImageUrl = "https://example.com/ipad.jpg"
            },
            new Listing
            {
                Title = "Gaming Laptop - ASUS",
                Description = "RTX 4070, i9, 32GB RAM, 1TB SSD, excellent gaming performance",
                Price = 1599.00m,
                UserId = bob.Id,
                CategoryId = electronics.Id,
                ImageUrl = "https://example.com/asus.jpg"
            },
            new Listing
            {
                Title = "Office Chair",
                Description = "Ergonomic gaming chair, adjustable, black",
                Price = 299.99m,
                UserId = bob.Id,
                CategoryId = furniture.Id,
                ImageUrl = "https://example.com/chair.jpg"
            },
            new Listing
            {
                Title = "Standing Desk",
                Description = "Electric, oak finish, 48x24 inches",
                Price = 599.00m,
                UserId = bob.Id,
                CategoryId = furniture.Id,
                ImageUrl = "https://example.com/desk.jpg"
            },
            new Listing
            {
                Title = "Bookshelf 5-Tier",
                Description = "Walnut wood, sturdy, holds lots of books",
                Price = 129.99m,
                UserId = bob.Id,
                CategoryId = furniture.Id,
                ImageUrl = "https://example.com/bookshelf.jpg"
            }
        };

        context.Listings.AddRange(listings);
        context.SaveChanges();
    }

    // ---- Add AuditLogs ----
    if (!context.AuditLogs.Any())
    {
        var user = context.Users.First(u => u.Username == "alice");
        var log = new AuditLog
        {
            UserId = user.Id,
            Action = "Created",
            EntityId = 1, // the Listing Id
            Details = "{\"Title\":\"iPhone 14\"}"
        };
        context.AuditLogs.Add(log);
        context.SaveChanges();
    }

    // ---- Print data to console ----
    var users = context.Users.ToList();
    var allListings = context.Listings.Include(l => l.User).Include(l => l.Category).ToList();

    foreach (var user in users)
        Console.WriteLine($"User: {user.Username}, Email: {user.Email}");

    foreach (var listing in allListings)
        Console.WriteLine($"Listing: {listing.Title}, Price: {listing.Price}, Owner: {listing.User.Username}");
}




app.MapControllers();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();

// Make Program class accessible to integration tests
public partial class Program { }
