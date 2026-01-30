using Microsoft.EntityFrameworkCore;
using MVC___MSSQL_Classifieds_Portal.Models;
using MVC___MSSQL_Classifieds_Portal.Services;

namespace MVC.Classifieds.Tests;

/// <summary>
/// Integration tests for database operations and service interactions
/// </summary>
public class ListingsIntegrationTests
{
    [Fact]
    public async Task Database_CanSaveAndRetrieve_Listing()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<ClassifieldsContext>()
            .UseInMemoryDatabase(databaseName: "TestDb_" + Guid.NewGuid())
            .Options;

        using var context = new ClassifieldsContext(options);
        
        var category = new Category { Id = 1, Name = "Electronics", CreatedAt = DateTime.UtcNow };
        var user = new User { Id = 1, Username = "testuser", Email = "test@test.com", PasswordHash = "hash", CreatedAt = DateTime.UtcNow };
        var listing = new Listing
        {
            Id = 1,
            Title = "Test Laptop",
            Description = "Test",
            Price = 500m,
            CategoryId = 1,
            UserId = 1,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        context.Categories.Add(category);
        context.Users.Add(user);
        context.Listings.Add(listing);
        await context.SaveChangesAsync();

        // Act
        var retrieved = await context.Listings
            .Include(l => l.Category)
            .Include(l => l.User)
            .FirstOrDefaultAsync(l => l.Id == 1);

        // Assert
        Assert.NotNull(retrieved);
        Assert.Equal("Test Laptop", retrieved.Title);
        Assert.Equal(500m, retrieved.Price);
        Assert.NotNull(retrieved.Category);
        Assert.Equal("Electronics", retrieved.Category.Name);
    }

    [Fact]
    public async Task Database_IndexedQuery_ByCategoryAndIsActive()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<ClassifieldsContext>()
            .UseInMemoryDatabase(databaseName: "TestDb_" + Guid.NewGuid())
            .Options;

        using var context = new ClassifieldsContext(options);
        
        var category = new Category { Id = 1, Name = "Electronics", CreatedAt = DateTime.UtcNow };
        var user = new User { Id = 1, Username = "testuser", Email = "test@test.com", PasswordHash = "hash", CreatedAt = DateTime.UtcNow };
        
        context.Categories.Add(category);
        context.Users.Add(user);
        
        for (int i = 1; i <= 5; i++)
        {
            context.Listings.Add(new Listing
            {
                Id = i,
                Title = $"Listing {i}",
                Price = i * 100m,
                CategoryId = 1,
                UserId = 1,
                IsActive = i % 2 == 0, // Even numbers are active
                CreatedAt = DateTime.UtcNow.AddDays(-i)
            });
        }
        await context.SaveChangesAsync();

        // Act - Query with composite index: CategoryId + IsActive
        var activeListings = await context.Listings
            .Where(l => l.CategoryId == 1 && l.IsActive)
            .ToListAsync();

        // Assert
        Assert.Equal(2, activeListings.Count);
        Assert.All(activeListings, l => Assert.True(l.IsActive));
    }

    [Fact]
    public async Task Database_QueryByPrice_UsesIndex()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<ClassifieldsContext>()
            .UseInMemoryDatabase(databaseName: "TestDb_" + Guid.NewGuid())
            .Options;

        using var context = new ClassifieldsContext(options);
        
        var category = new Category { Id = 1, Name = "Electronics", CreatedAt = DateTime.UtcNow };
        var user = new User { Id = 1, Username = "testuser", Email = "test@test.com", PasswordHash = "hash", CreatedAt = DateTime.UtcNow };
        
        context.Categories.Add(category);
        context.Users.Add(user);
        context.Listings.AddRange(
            new Listing { Title = "Cheap Item", Price = 50m, CategoryId = 1, UserId = 1, IsActive = true, CreatedAt = DateTime.UtcNow },
            new Listing { Title = "Mid Item", Price = 500m, CategoryId = 1, UserId = 1, IsActive = true, CreatedAt = DateTime.UtcNow },
            new Listing { Title = "Expensive Item", Price = 2000m, CategoryId = 1, UserId = 1, IsActive = true, CreatedAt = DateTime.UtcNow }
        );
        await context.SaveChangesAsync();

        // Act - Query with Price index
        var affordableListings = await context.Listings
            .Where(l => l.Price <= 500m)
            .OrderBy(l => l.Price)
            .ToListAsync();

        // Assert
        Assert.Equal(2, affordableListings.Count);
        Assert.Equal(50m, affordableListings.First().Price);
    }

    [Fact]
    public async Task AuditLogService_LogsCorrectly()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<ClassifieldsContext>()
            .UseInMemoryDatabase(databaseName: "TestDb_" + Guid.NewGuid())
            .Options;

        using var context = new ClassifieldsContext(options);
        var auditService = new AuditLogService(context);

        // Act
        await auditService.LogAsync("CREATE", "Listing", 1, 1, new { Title = "Test", Price = 100m });

        // Assert
        var logs = await context.AuditLogs.ToListAsync();
        Assert.Single(logs);
        Assert.Equal("CREATE", logs[0].Action);
        Assert.Equal("Listing", logs[0].EntityName);
        Assert.Equal(1, logs[0].EntityId);
        Assert.Contains("Test", logs[0].Changes);
    }

    [Fact]
    public async Task AuditLog_QueryByTimestamp_OrdersCorrectly()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<ClassifieldsContext>()
            .UseInMemoryDatabase(databaseName: "TestDb_" + Guid.NewGuid())
            .Options;

        using var context = new ClassifieldsContext(options);
        
        var baseTime = DateTime.UtcNow;
        context.AuditLogs.AddRange(
            new AuditLog { Action = "CREATE", EntityName = "Listing", EntityId = 1, UserId = 1, Timestamp = baseTime.AddMinutes(-5) },
            new AuditLog { Action = "UPDATE", EntityName = "Listing", EntityId = 1, UserId = 1, Timestamp = baseTime.AddMinutes(-3) },
            new AuditLog { Action = "DELETE", EntityName = "Listing", EntityId = 1, UserId = 1, Timestamp = baseTime }
        );
        await context.SaveChangesAsync();

        // Act - Query with descending Timestamp index
        var recentLogs = await context.AuditLogs
            .OrderByDescending(a => a.Timestamp)
            .Take(2)
            .ToListAsync();

        // Assert
        Assert.Equal(2, recentLogs.Count);
        Assert.Equal("DELETE", recentLogs[0].Action); // Most recent
        Assert.Equal("UPDATE", recentLogs[1].Action);
    }

    [Fact]
    public async Task Database_CascadeDelete_WorksCorrectly()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<ClassifieldsContext>()
            .UseInMemoryDatabase(databaseName: "TestDb_" + Guid.NewGuid())
            .Options;

        using var context = new ClassifieldsContext(options);
        
        var category = new Category { Id = 1, Name = "Electronics", CreatedAt = DateTime.UtcNow };
        var user = new User { Id = 1, Username = "testuser", Email = "test@test.com", PasswordHash = "hash", CreatedAt = DateTime.UtcNow };
        var listing = new Listing
        {
            Title = "Test Item",
            Price = 100m,
            CategoryId = 1,
            UserId = 1,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        context.Categories.Add(category);
        context.Users.Add(user);
        context.Listings.Add(listing);
        await context.SaveChangesAsync();

        var listingId = listing.Id;

        // Act - Soft delete (set IsActive to false)
        listing.IsActive = false;
        await context.SaveChangesAsync();

        // Assert
        var deletedListing = await context.Listings.FindAsync(listingId);
        Assert.NotNull(deletedListing);
        Assert.False(deletedListing.IsActive);
    }

    [Fact]
    public async Task Database_SearchByTitle_ReturnsMatchingListings()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<ClassifieldsContext>()
            .UseInMemoryDatabase(databaseName: "TestDb_" + Guid.NewGuid())
            .Options;

        using var context = new ClassifieldsContext(options);
        
        var category = new Category { Id = 1, Name = "Electronics", CreatedAt = DateTime.UtcNow };
        var user = new User { Id = 1, Username = "testuser", Email = "test@test.com", PasswordHash = "hash", CreatedAt = DateTime.UtcNow };
        
        context.Categories.Add(category);
        context.Users.Add(user);
        context.Listings.AddRange(
            new Listing { Title = "Laptop Dell", Price = 500m, CategoryId = 1, UserId = 1, IsActive = true, CreatedAt = DateTime.UtcNow },
            new Listing { Title = "Laptop HP", Price = 600m, CategoryId = 1, UserId = 1, IsActive = true, CreatedAt = DateTime.UtcNow },
            new Listing { Title = "Mouse Logitech", Price = 20m, CategoryId = 1, UserId = 1, IsActive = true, CreatedAt = DateTime.UtcNow }
        );
        await context.SaveChangesAsync();

        // Act - Search with Title index
        var laptops = await context.Listings
            .Where(l => l.Title.Contains("Laptop"))
            .ToListAsync();

        // Assert
        Assert.Equal(2, laptops.Count);
        Assert.All(laptops, l => Assert.Contains("Laptop", l.Title));
    }
}
