using System.Security.Claims;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using MVC___MSSQL_Classifieds_Portal.Controllers;
using MVC___MSSQL_Classifieds_Portal.Models;
using MVC___MSSQL_Classifieds_Portal.Services;

namespace MVC.Classifieds.Tests;

public class ListingsControllerTests
{
    private readonly ClassifieldsContext _context;
    private readonly Mock<IAuditLogService> _mockAuditLog;
    private readonly Mock<IMapper> _mockMapper;
    private readonly ListingsController _controller;

    public ListingsControllerTests()
    {
        // Setup in-memory database
        var options = new DbContextOptionsBuilder<ClassifieldsContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ClassifieldsContext(options);
        _mockAuditLog = new Mock<IAuditLogService>();
        _mockMapper = new Mock<IMapper>();
        _controller = new ListingsController(_context, _mockMapper.Object, _mockAuditLog.Object);

        // Seed test data
        SeedTestData();
    }

    private void SeedTestData()
    {
        var category = new Category
        {
            Id = 1,
            Name = "Electronics"
        };

        var user = new User
        {
            Id = 1,
            Username = "testuser",
            Email = "test@example.com",
            PasswordHash = "hashedpassword",
            CreatedAt = DateTime.UtcNow
        };

        _context.Categories.Add(category);
        _context.Users.Add(user);
        _context.SaveChanges();
    }

    private void SetupUserClaims(int userId, string username)
    {
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
            new Claim(ClaimTypes.Name, username)
        };
        var identity = new ClaimsIdentity(claims, "TestAuthType");
        var claimsPrincipal = new ClaimsPrincipal(identity);

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = claimsPrincipal }
        };
    }

    [Fact]
    public async Task Index_ReturnsViewWithListings()
    {
        // Arrange
        var listing = new Listing
        {
            Id = 1,
            Title = "Test Listing",
            Description = "Test Description",
            Price = 100.00m,
            CategoryId = 1,
            UserId = 1,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
        _context.Listings.Add(listing);
        await _context.SaveChangesAsync();

        // Act
        var result = await _controller.Index(null, null, null, null, "1");

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.NotNull(viewResult.Model);
    }

    [Fact]
    public async Task Create_ValidListing_RedirectsToIndex()
    {
        // Arrange
        SetupUserClaims(1, "testuser");
        var listing = new Listing
        {
            Title = "New Laptop",
            Description = "Brand new laptop for sale",
            Price = 999.99m,
            CategoryId = 1,
            IsActive = true
        };

        // Act
        var result = await _controller.Create(listing);

        // Assert
        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Index", redirectResult.ActionName);
        
        // Verify listing was created
        var createdListing = await _context.Listings.FirstOrDefaultAsync(l => l.Title == "New Laptop");
        Assert.NotNull(createdListing);
        Assert.Equal(1, createdListing.UserId);
        Assert.Equal(999.99m, createdListing.Price);

        // Verify audit log was called
        _mockAuditLog.Verify(x => x.LogAsync(
            "CREATE",
            "Listing",
            It.IsAny<int>(),
            1,
            It.IsAny<object>()
        ), Times.Once);
    }

    [Fact]
    public async Task Create_InvalidModelState_ReturnsViewWithModel()
    {
        // Arrange
        SetupUserClaims(1, "testuser");
        _controller.ModelState.AddModelError("Title", "Title is required");
        var listing = new Listing();

        // Act
        var result = await _controller.Create(listing);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal(listing, viewResult.Model);
    }

    [Fact]
    public async Task Edit_ValidListing_UpdatesAndRedirects()
    {
        // Arrange
        SetupUserClaims(1, "testuser");
        var listing = new Listing
        {
            Id = 10,
            Title = "Old Title",
            Description = "Old Description",
            Price = 100.00m,
            CategoryId = 1,
            UserId = 1,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
        _context.Listings.Add(listing);
        await _context.SaveChangesAsync();

        // Detach to avoid tracking issues
        _context.Entry(listing).State = EntityState.Detached;

        var updatedListing = new Listing
        {
            Id = 10,
            Title = "Updated Title",
            Description = "Updated Description",
            Price = 200.00m,
            CategoryId = 1,
            UserId = 1,
            IsActive = true,
            CreatedAt = listing.CreatedAt
        };

        // Act
        var result = await _controller.Edit(10, updatedListing);

        // Assert
        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Index", redirectResult.ActionName);

        // Verify listing was updated
        var modifiedListing = await _context.Listings.FindAsync(10);
        Assert.NotNull(modifiedListing);
        Assert.Equal("Updated Title", modifiedListing.Title);
        Assert.Equal(200.00m, modifiedListing.Price);

        // Verify audit log was called
        _mockAuditLog.Verify(x => x.LogAsync(
            "UPDATE",
            "Listing",
            10,
            1,
            It.IsAny<object>()
        ), Times.Once);
    }

    [Fact]
    public async Task Edit_UnauthorizedUser_ReturnsForbid()
    {
        // Arrange
        SetupUserClaims(2, "otheruser"); // Different user
        var listing = new Listing
        {
            Id = 20,
            Title = "Test Listing",
            Description = "Test Description",
            Price = 100.00m,
            CategoryId = 1,
            UserId = 1, // Owned by user 1
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
        _context.Listings.Add(listing);
        await _context.SaveChangesAsync();

        var updatedListing = new Listing
        {
            Id = 20,
            Title = "Hacked Title",
            Description = "Hacked",
            Price = 1.00m,
            CategoryId = 1,
            UserId = 1,
            IsActive = true,
            CreatedAt = listing.CreatedAt
        };

        // Act
        var result = await _controller.Edit(20, updatedListing);

        // Assert
        Assert.IsType<ForbidResult>(result);
    }

    [Fact]
    public async Task DeleteConfirmed_ValidListing_SoftDeletesAndRedirects()
    {
        // Arrange
        SetupUserClaims(1, "testuser");
        var listing = new Listing
        {
            Id = 30,
            Title = "To Delete",
            Description = "This will be deleted",
            Price = 50.00m,
            CategoryId = 1,
            UserId = 1,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
        _context.Listings.Add(listing);
        await _context.SaveChangesAsync();

        // Act
        var result = await _controller.DeleteConfirmed(30);

        // Assert
        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Index", redirectResult.ActionName);

        // Verify soft delete
        var deletedListing = await _context.Listings.FindAsync(30);
        Assert.NotNull(deletedListing);
        Assert.False(deletedListing.IsActive);

        // Verify audit log was called
        _mockAuditLog.Verify(x => x.LogAsync(
            "DELETE",
            "Listing",
            30,
            1,
            It.IsAny<object>()
        ), Times.Once);
    }

    [Fact]
    public async Task DeleteConfirmed_UnauthorizedUser_ReturnsForbid()
    {
        // Arrange
        SetupUserClaims(2, "otheruser"); // Different user
        var listing = new Listing
        {
            Id = 40,
            Title = "Protected Listing",
            Description = "Can't delete this",
            Price = 100.00m,
            CategoryId = 1,
            UserId = 1, // Owned by user 1
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
        _context.Listings.Add(listing);
        await _context.SaveChangesAsync();

        // Act
        var result = await _controller.DeleteConfirmed(40);

        // Assert
        Assert.IsType<ForbidResult>(result);
        
        // Verify listing is still active
        var stillActiveListing = await _context.Listings.FindAsync(40);
        Assert.NotNull(stillActiveListing);
        Assert.True(stillActiveListing.IsActive);
    }

    [Fact]
    public async Task Details_ExistingListing_ReturnsViewWithListing()
    {
        // Arrange
        var listing = new Listing
        {
            Id = 50,
            Title = "Details Test",
            Description = "Test Description",
            Price = 100.00m,
            CategoryId = 1,
            UserId = 1,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
        _context.Listings.Add(listing);
        await _context.SaveChangesAsync();

        // Act
        var result = await _controller.Details(50);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<Listing>(viewResult.Model);
        Assert.Equal("Details Test", model.Title);
    }

    [Fact]
    public async Task Details_NonExistingListing_ReturnsNotFound()
    {
        // Act
        var result = await _controller.Details(999);

        // Assert
        Assert.IsType<NotFoundResult>(result);
    }
}
