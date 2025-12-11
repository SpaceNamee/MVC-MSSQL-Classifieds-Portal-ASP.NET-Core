using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using MVC___MSSQL_Classifieds_Portal.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = null;
    });


builder.Services.AddDbContext<ClassifieldsContext>(options =>
    options.UseSqlServer("server=DESKTOP-5CVM6RG\\SQLEXPRESS01;Database=ClassifieldsContext;Trusted_Connection=True;TrustServerCertificate=True")
);


var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ClassifieldsContext>();

    // Ensure database is created
    context.Database.EnsureCreated();

    // ---- Add Users ----
    if (!context.Users.Any()) // prevent duplicate inserts.
    {
        var user1 = new User
        {
            Username = "alice",
            Email = "alice@example.com",
            PasswordHash = "hashedpassword1"
        };
        var user2 = new User
        {
            Username = "bob",
            Email = "bob@example.com",
            PasswordHash = "hashedpassword2"
        };
        context.Users.AddRange(user1, user2);
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
        var user = context.Users.First(u => u.Username == "alice");
        var category = context.Categories.First(c => c.Name == "Electronics");

        var listing1 = new Listing
        {
            Title = "iPhone 14",
            Description = "Like new, 256GB",
            Price = 799.99m,
            UserId = user.Id,
            CategoryId = category.Id,
            ImageUrl = "https://example.com/iphone14.jpg"
        };
        context.Listings.Add(listing1);
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
    var users = context.Users.ToList();
    var listings = context.Listings.Include(l => l.User).Include(l => l.Category).ToList();

    foreach (var user in users)
        Console.WriteLine($"User: {user.Username}, Email: {user.Email}");

    foreach (var listing in listings)
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

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
